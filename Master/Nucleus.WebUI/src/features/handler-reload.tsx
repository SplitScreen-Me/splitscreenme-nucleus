// Handler hot reload (Nucleus 2.5): a script added, edited, fixed or deleted in the handlers folder is picked up by the
// host while Nucleus runs, and the lists refresh through `handlers.changed` / `library.changed`. This says so in a short
// toast. Broken scripts are notices (the notice dialog), never toasts.
import { useBridgeEvent, type HandlerReloadKind } from '@/bridge';
import { useI18n, type MessageKey } from '@/i18n';
import { useToast, type IconName } from '@/ui';

const TEXT: Record<HandlerReloadKind, MessageKey> = {
  added: 'features.handlerReload.added',
  updated: 'features.handlerReload.updated',
  fixed: 'features.handlerReload.fixed',
  removed: 'features.handlerReload.removed',
};

const ICON: Record<HandlerReloadKind, IconName> = { added: 'plus', updated: 'refresh', fixed: 'check', removed: 'info' };

/** Toasts handlers reloaded from the handlers folder. Mounted once, inside the toast provider. */
export function HandlerReloadToasts() {
  const toast = useToast();
  const { t } = useI18n();
  useBridgeEvent('handlers.changed', (e) => {
    const reloaded = e.reloaded ?? [];
    const [only] = reloaded;
    if (!only) return;
    if (reloaded.length > 1) toast(t('features.handlerReload.several', { count: reloaded.length }), { icon: 'refresh' });
    else toast(t(TEXT[only.kind], { game: only.gameName }), { icon: ICON[only.kind], durationMs: only.kind === 'removed' ? 4200 : undefined });
  });
  return null;
}
