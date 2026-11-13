-- Whether a lambda may connect to other servers: its egress.
--
-- Lambdas were used as proxies - an API of their own that fetched whatever
-- address it was given - so a lambda that may not connect is compiled without
-- the means to: HttpClient, sockets, WebSocket clients, mail, DNS. A premium
-- lambda may, whatever is written here; any other once its owner asked and
-- the operator allowed it.
--
-- egress is where the request stands: Requested, Allowed or Declined, or
-- null while nobody asked. egress_changed says since when, egress_purpose
-- what the owner said the lambda connects to and why - kept once it is
-- decided, so the operator can read later what they allowed - and
-- egress_note what the operator said deciding it, which the owner reads.
--
-- Nothing is allowed here for the lambdas that exist: the operator allows the
-- ones that need it by hand.

ALTER TABLE lambdas ADD COLUMN egress TEXT NULL;
ALTER TABLE lambdas ADD COLUMN egress_changed TEXT NULL;
ALTER TABLE lambdas ADD COLUMN egress_purpose TEXT NULL;
ALTER TABLE lambdas ADD COLUMN egress_note TEXT NULL;
