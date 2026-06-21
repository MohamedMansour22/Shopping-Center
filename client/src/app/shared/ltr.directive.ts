import { Directive } from '@angular/core';

/**
 * Isolates inherently-LTR data (phone numbers, emails, street addresses, dates) from the
 * surrounding paragraph direction, so the Unicode bidi algorithm can't reorder it when the
 * page is in Arabic/RTL mode.
 *
 * It forces an LTR base direction and isolates the run (`unicode-bidi: isolate`), so values
 * like "+1 555 0102" or "64 alhayeth st" keep their correct character order inside an RTL
 * layout instead of rendering as "0102 555 1+" / "alhayeth 64 st".
 *
 * Apply to a wrapper around the value, e.g. `<span appLtr>{{ order.customerPhone }}</span>`.
 * The host stays inline, so it doesn't disturb the surrounding block's alignment — only the
 * wrapped value is treated as a single isolated LTR unit.
 */
@Directive({
  selector: '[appLtr]',
  host: {
    dir: 'ltr',
    '[style.unicode-bidi]': "'isolate'",
  },
})
export class LtrDirective {}
