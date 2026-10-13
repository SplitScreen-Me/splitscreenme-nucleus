// Dropping a handler file (.nc, .zip, .js) anywhere installs it, then offers to add its game.
// The WebView must never navigate to a dropped file: every file drag is intercepted at the window level.
import { useEffect, useRef, useState } from 'react';
import { bridge } from '@/bridge';
import { t as translate, useT } from '@/i18n';
import { errorMessage, isCancelled, refusalReason } from '@/lib/bridge-ui';
import { installHandlerFile } from '@/lib/downloads';
import { Icon, useConfirm, useToast } from '@/ui';
import { useAddGame } from './add-game';

const HANDLER_FILE = /\.(nc|zip|js)$/i;

const hasFiles = (e: DragEvent) => Boolean(e.dataTransfer?.types.includes('Files'));

/** Window-wide drop target for handler files, with its overlay. Mounted once by the shell. */
export function FileDrop() {
  const [over, setOver] = useState(false);
  const depth = useRef(0);
  const t = useT();
  const toast = useToast();
  const confirm = useConfirm();
  const { afterInstall } = useAddGame();
  const live = useRef({ toast, confirm, afterInstall });
  live.current = { toast, confirm, afterInstall };

  // Texts built when the events happen, in the language in use then.
  useEffect(() => {
    const install = async (path: string) => {
      const { toast: say, confirm: ask, afterInstall: addGame } = live.current;
      const file = path.slice(Math.max(path.lastIndexOf('\\'), path.lastIndexOf('/')) + 1);
      try {
        let result;
        try {
          result = await installHandlerFile(path);
        } catch (error) {
          if (refusalReason(error) !== 'alreadyInstalled') throw error;
          const game = (error as { data?: { gameName?: string } }).data?.gameName;
          const replace = await ask({
            title: translate('features.fileDrop.replace.title'),
            message: game
              ? translate('features.fileDrop.replace.message', { game, file })
              : translate('features.fileDrop.replace.messageUnknownGame', { file }),
            confirmLabel: translate('features.fileDrop.replace.confirm'),
            icon: 'update',
          });
          if (!replace) return;
          result = await installHandlerFile(path, true);
        }
        say(translate('features.fileDrop.installed', { game: result.gameName }));
        await addGame(result);
      } catch (error) {
        if (!isCancelled(error)) say(errorMessage(error), { icon: 'warning', durationMs: 4200 });
      }
    };

    const onDragEnter = (e: DragEvent) => {
      if (!hasFiles(e)) return;
      e.preventDefault();
      depth.current += 1;
      setOver(true);
    };
    const onDragOver = (e: DragEvent) => {
      if (!hasFiles(e)) return;
      e.preventDefault();
      if (e.dataTransfer) e.dataTransfer.dropEffect = 'copy';
    };
    const onDragLeave = (e: DragEvent) => {
      if (!hasFiles(e)) return;
      depth.current = Math.max(0, depth.current - 1);
      if (depth.current === 0) setOver(false);
    };
    const onDrop = (e: DragEvent) => {
      if (!hasFiles(e)) return;
      e.preventDefault();
      depth.current = 0;
      setOver(false);
      const files = [...(e.dataTransfer?.files ?? [])];
      const handlers = files.filter((f) => HANDLER_FILE.test(f.name));
      if (handlers.length === 0) {
        live.current.toast(translate('features.fileDrop.wrongType'), { icon: 'info' });
        return;
      }
      void bridge
        .getFilePaths(handlers)
        .then(async (paths) => {
          for (const path of paths) await install(path);
        })
        // Files that are not on disk (dragged from a browser, a zip viewer…) have no path the app can open.
        .catch(() =>
          live.current.toast(translate('features.fileDrop.noPath'), {
            icon: 'warning',
            durationMs: 5000,
          }),
        );
    };

    window.addEventListener('dragenter', onDragEnter);
    window.addEventListener('dragover', onDragOver);
    window.addEventListener('dragleave', onDragLeave);
    window.addEventListener('drop', onDrop);
    return () => {
      window.removeEventListener('dragenter', onDragEnter);
      window.removeEventListener('dragover', onDragOver);
      window.removeEventListener('dragleave', onDragLeave);
      window.removeEventListener('drop', onDrop);
    };
  }, []);

  if (!over) return null;
  return (
    <div aria-hidden="true" className="pointer-events-none fixed inset-0 z-[60] grid place-items-center bg-black/70 p-10 animate-fade-in">
      <div className="grid size-full place-content-center justify-items-center gap-4 rounded-[18px] border-2 border-dashed border-ink/70 text-center">
        <span className="grid size-16 place-items-center rounded-full bg-ink text-on-light">
          <Icon name="download" className="size-8" />
        </span>
        <strong className="type-h2">{t('features.fileDrop.overlay.title')}</strong>
        <span className="text-ink-2">{t('features.fileDrop.overlay.hint')}</span>
      </div>
    </div>
  );
}
