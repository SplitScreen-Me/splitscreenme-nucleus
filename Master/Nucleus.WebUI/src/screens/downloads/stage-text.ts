import type { DownloadItem } from '@/lib/downloads';

/** "Waiting…", "Downloading… 45%", "Installing…", "Installed", "Failed", "Cancelled". */
export function stageText(item: Pick<DownloadItem, 'stage' | 'percent'>): string {
  switch (item.stage) {
    case 'queued':
      return 'Waiting…';
    case 'downloading':
      return item.percent !== undefined ? `Downloading… ${Math.round(item.percent)}%` : 'Downloading…';
    case 'extracting':
      return 'Installing…';
    case 'done':
      return 'Installed';
    case 'failed':
      return 'Failed';
    case 'cancelled':
      return 'Cancelled';
  }
}
