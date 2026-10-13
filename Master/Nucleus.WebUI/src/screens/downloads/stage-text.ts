import type { Formatters, TFunction } from '@/i18n';
import type { DownloadItem } from '@/lib/downloads';

/** "Waiting…", "Downloading… 45%", "Installing…", "Installed", "Failed", "Cancelled". Pass `t` and `format` of `useI18n()`. */
export function stageText(t: TFunction, format: Formatters, item: Pick<DownloadItem, 'stage' | 'percent'>): string {
  switch (item.stage) {
    case 'queued':
      return t('downloads.stage.queued');
    case 'downloading':
      return item.percent !== undefined
        ? t('downloads.stage.downloadingPercent', { percent: format.percent(Math.round(item.percent) / 100) })
        : t('downloads.stage.downloading');
    case 'extracting':
      return t('downloads.stage.extracting');
    case 'done':
      return t('downloads.stage.done');
    case 'failed':
      return t('downloads.stage.failed');
    case 'cancelled':
      return t('downloads.stage.cancelled');
  }
}
