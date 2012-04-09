Two-Tier Security
=================

After many hours poking around at MVC, WCF, and WIF I'm posting the code that finally had delegation working across tiers.

This code applies basic federated authentication between an security token service (STS) and relying party (RP).  Both the STS and RP are built with MVC.  Next up is a WCF service which wants the MVC application to delegate the current user credentials down to it.  This was trickier part in terms of spinning up a WS-Trust endpoint inside the STS and figuring out how to flow the UI credentials across with the user token.

**NOTE** Transport security, certificate verification and encryption are completely turned off. This was so I could focus on the core point of identity flow rather than the infrastructure to secure it.