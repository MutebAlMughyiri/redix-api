[Documentation](README.md) › Environments

# Environments

Every REDIX environment: where to open it, what data it holds, and which app talks to it. Each instance answers straight from its own process on its own port: no proxy, no redirect, no layer in between (redix-api #80, D3; the owner, 20 Nov 2026).

## Where to open each one

| Environment | Web, in a browser | API, for the app | Data |
| --- | --- | --- | --- |
| **Production** | https://redix.muteb.net | https://redix.muteb.net:8443 | The real projects; it started empty on 20 Nov 2026 |
| **qa** | https://qa.redix.muteb.net:9444 | https://qa.redix.muteb.net:8444 | A copy of the owner's qa data, for testers |
| **dev** | https://dev.redix.muteb.net:9445 | https://dev.redix.muteb.net:8445 | Empty, for trying things |
| PC, dev | http://127.0.0.1:5020 | http://127.0.0.1:5010 | `RedixDev.db` on the PC, for automated runs |
| PC, qa | http://127.0.0.1:5022 | http://127.0.0.1:5012 | `RedixQA.db` on the PC, for the owner's own testing |

**The web opens on dev and qa:** the PC's dev and qa, the server's dev since 21 Nov 2026 (https://dev.redix.muteb.net:9445), where redix-web-automation runs, and the server's qa since 23 Nov 2026 (https://qa.redix.muteb.net:9444), which the owner opened to see the web; automation never runs against it. Production's web stays closed until the owner opens it: its settings' `REDIX_Web__Urls`, then a deploy, which opens the port. The API column answers today: add `/health` to an address to see it is up, as in https://redix.muteb.net:8443/health.

Production's web address has no port because 443 is the browser's own. qa and dev have their own ports since one port can belong to one process only, and a browser keeps its sign-in by name, so each environment keeps its own name and its sessions stay apart. On the PC everything is plain `http`, as nothing leaves the machine; 5011 belongs to Patrol's PC helper.

## Which app talks to which

| App on the phone | Installed from | Talks to |
| --- | --- | --- |
| REDIX (`com.techdetails.redix`) | The stores, once published; App Tester's `prod` group until then | Production |
| REDIX qa (`com.techdetails.redix.qa`) | App Tester, group `qa` | qa |
| REDIX dev (`com.techdetails.redix.dev`) | App Tester, group `dev` | dev |
| REDIX qa local (`com.techdetails.redix.qa.local`) | Built on the PC (`config/qa.json`) | PC, qa, through `adb reverse tcp:5012 tcp:5012` |
| REDIX dev local (`com.techdetails.redix.dev.local`) | Built on the PC by Patrol (`config/dev.json`) | PC, dev, through `adb reverse tcp:5010 tcp:5010` |

The test builds are redix-app's "Test builds" workflow (redix-app #118); how the server's instances are set up is go-backend's README, "Deploying".
