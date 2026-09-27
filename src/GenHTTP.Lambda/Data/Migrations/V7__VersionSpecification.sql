-- The note on a version held "the prompt": what an agent was asked, word for
-- word. Asking an agent to hand over its prompt is asking for more than the
-- owner needs and more than it should give. What the owner needs is the why -
-- what the user wanted and the requirements behind it - so the note is the
-- specification the version answers, told in the user's words where possible.
--
-- The column keeps what is there: an old prompt is still a fair account of
-- what was wanted.

ALTER TABLE deployments RENAME COLUMN prompt TO specification;
