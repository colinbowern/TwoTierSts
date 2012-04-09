Two-Tier Security
=================

After many hours poking around at MVC, WCF, and WIF I'm posting the code that finally had delegation working across tiers.

This code applies basic federated authentication between an security token service (STS) and relying party (RP).  Both the STS and RP are built with MVC.  Next up is a WCF service which wants the MVC application to delegate the current user credentials down to it.  This was trickier part in terms of spinning up a WS-Trust endpoint inside the STS and figuring out how to flow the UI credentials across with the user token.

This code is not for production use, but instead meant to be a learning tool.  You will also want to check out the work of Dominick Baier, Pablo M. Cibraro, and Vittorio Bertocci among others to further explore WIF.

**NOTE** Transport security, certificate verification and encryption are completely turned off. This was so I could focus on the core point of identity flow rather than the infrastructure to secure it.