-- The branch a feature is in the lambda's git repository.
--
-- Every lambda can be cloned with git: its versions are the commits of main,
-- and each feature is a branch. A branch has a name that git clients keep -
-- in their remote, in their checkouts - so it is decided once, when the
-- feature starts, and kept: renaming a feature changes what the editor calls
-- it, never where it is in a clone. A feature started by pushing a branch is
-- called what the branch was called; one started anywhere else gets its name
-- in the words git allows.
--
-- The features open now never had one, and get one from their id: they are
-- few, and nobody has cloned them yet.

ALTER TABLE features ADD COLUMN branch TEXT NULL;

UPDATE features SET branch = 'feature-' || id WHERE branch IS NULL;

CREATE UNIQUE INDEX ix_features_branch ON features (lambda_id, branch);
