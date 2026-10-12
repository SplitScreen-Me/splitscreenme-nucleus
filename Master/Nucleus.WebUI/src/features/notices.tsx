// Notices the host raises outside a session (handler load or script errors, an unloaded profile, locked game files):
// one at a time, oldest first, until the user dismisses them. Held back while a session is live: they show after it.
import { useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { bridge, bridgeQueryKey, useBridgeQuery, type NoticeLevel } from '@/bridge';
import { isSessionLive, useSessionSnapshot } from '@/lib/live';
import { Button, Dialog, type IconName } from '@/ui';

const TONE: Record<NoticeLevel, { icon: IconName; className: string }> = {
  info: { icon: 'info', className: 'text-ink-2' },
  warning: { icon: 'warning', className: 'text-warn' },
  error: { icon: 'warning', className: 'text-danger' },
};

/** The oldest waiting notice, in a modal. Mounted once by the shell. */
export function NoticeDialog() {
  const queryClient = useQueryClient();
  const { data: notices } = useBridgeQuery('notices.list');
  const { data: session } = useSessionSnapshot();
  // Dismissed here: hidden at once, whatever the host answers (a failed dismiss must not trap the user in the modal).
  const [dismissed, setDismissed] = useState<ReadonlySet<string>>(new Set());

  const waiting = (notices ?? []).filter((n) => !dismissed.has(n.id));
  const notice = isSessionLive(session?.state) ? undefined : waiting[0];
  const tone = TONE[notice?.level ?? 'info'];

  const dismiss = (id: string) => {
    setDismissed((old) => new Set(old).add(id));
    bridge.notices.dismiss({ id }).catch(() => queryClient.invalidateQueries({ queryKey: bridgeQueryKey('notices.list') }));
  };

  return (
    <Dialog
      open={notice !== undefined}
      // Escape, the close button and the backdrop dismiss too; closing because the notice went away does nothing.
      onClose={() => notice && dismiss(notice.id)}
      title={notice?.title ?? ''}
      icon={tone.icon}
      iconClassName={tone.className}
      headerExtra={waiting.length > 1 && <span className="text-12 text-ink-3 tabular-nums">1 of {waiting.length}</span>}
    >
      {notice && (
        <div className="grid gap-6">
          <p className="max-h-[50vh] overflow-auto text-15 leading-[1.55] whitespace-pre-line text-ink-2 wrap-anywhere">{notice.message}</p>
          <div className="flex justify-end">
            <Button variant="primary" onClick={() => dismiss(notice.id)} data-nav autoFocus>
              OK
            </Button>
          </div>
        </div>
      )}
    </Dialog>
  );
}
