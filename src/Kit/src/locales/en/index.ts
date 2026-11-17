import { abuse, common, notFound, shell } from './common';
import { buildBox } from './buildBox';
import { connect } from './connect';
import { kinds, licenses } from './licenses';
import { card, showcase } from './showcase';
import { workspace } from './workspace';

/**
 * The words every page of the platform shares, in English, which every other
 * language translates: the shell around the pages, what is said in many
 * places, connecting an agent, a card of the showcase and the licenses a
 * source is published under. Each bundle puts them together with
 * its own words into the catalog its pages read (see i18n).
 */
export const kit = {
  shell,
  common,
  connect,
  buildBox,
  workspace,
  notFound,
  abuse,
  showcase,
  card,
  licenses,
  kinds,
};

export type KitMessages = typeof kit;
