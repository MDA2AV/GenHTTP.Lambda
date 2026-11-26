#!/usr/bin/env bash
#
# Starts the CLI inside the agent's container, with the flags and the tool
# lists builder.mjs gives a build, and prints the tools it says it loaded -
# failing if any of them is not one of the platform's MCP tools.
#
# Run this after changing CLAUDE_VERSION. The allowlist is enforced by the
# CLI, but some built-in tools are offered whatever the allowlist says, so
# builds load none (--tools "") and the denylist names them as well - and
# both are only known to hold for the version they were checked against.
#
# The CLI says which tools it loaded in the first line it writes, before it
# asks the model anything, so this stops it there: nothing is spent, and it
# works before the credentials are set.
#
# Run on genhttp.dev, against the runner deploy/cloud started - with
# --search where the runner searches the web (AGENT_SEARCH=true), which loads
# the CLI's own WebSearch as well.
set -euo pipefail

SEARCH=false

if [[ "${1:-}" == "--search" ]]; then
  SEARCH=true
  shift
fi

SERVICE=${1:-agent}

COMPOSE=(docker compose -f "$(dirname "$(readlink -f "$0")")/../../deploy/cloud/docker-compose.yml")

echo "Asking the CLI in the agent's container which tools a build loads..."

"${COMPOSE[@]}" exec -T -e VERIFY_SEARCH="$SEARCH" "$SERVICE" node -e '
  const { spawn } = require("node:child_process");
  const fs = require("node:fs");
  const os = require("node:os");
  const path = require("node:path");

  // the lists as builder.mjs has them: the names between the brackets
  const source = fs.readFileSync("/app/builder.mjs", "utf8");
  const list = name => {
    const found = new RegExp(`const ${name} = \\[([\\s\\S]*?)\\];`).exec(source);
    if (!found) throw new Error(`no ${name} in builder.mjs`);
    return [...found[1].matchAll(/["\x27]([A-Za-z_][A-Za-z0-9_]*)["\x27]/g)].map(m => m[1]);
  };
  const search = process.env.VERIFY_SEARCH === "true";
  const deny = list("DENY").filter(t => !(search && t === "WebSearch"));
  const allow = [...new Set([...list("ALLOW"), ...list("CHANGE_ALLOW")])];

  const work = fs.mkdtempSync(path.join(os.tmpdir(), "verify-"));
  fs.writeFileSync(path.join(work, "mcp.json"),
    JSON.stringify({ mcpServers: { genhttp: { type: "http", url: process.env.AGENT_MCP_URL } } }));

  const child = spawn("claude", [
    "-p", "List every tool you can call.",
    "--mcp-config", "mcp.json", "--strict-mcp-config",
    "--model", "claude-haiku-5-5", "--effort", "low",
    "--permission-mode", "dontAsk", "--max-turns", "1",
    "--output-format", "stream-json", "--verbose",
    "--tools", search ? "WebSearch" : "",
    "--allowedTools", ...allow,
    "--disallowedTools", ...deny
  ], { cwd: work, stdio: ["ignore", "pipe", "inherit"] });

  let buffer = "";
  let seen = false;

  child.stdout.on("data", chunk => {
    buffer += chunk;
    let cut;
    while ((cut = buffer.indexOf("\n")) >= 0) {
      const line = buffer.slice(0, cut);
      buffer = buffer.slice(cut + 1);
      let event;
      try { event = JSON.parse(line); } catch { continue; }
      if (event.type !== "system" || event.subtype !== "init") continue;

      seen = true;
      child.kill();

      const tools = event.tools ?? [];
      // the platform'"'"'s MCP, the page fetch and hand-over beside it, and the
      // search where the runner searches
      const foreign = tools.filter(t => !t.startsWith("mcp__genhttp__") && t !== "mcp__web__fetch" && t !== "mcp__web__hand_over"
        && !(search && t === "WebSearch"));

      console.log(`CLI ${event.claude_code_version ?? "?"}, ${tools.length} tools loaded:`);
      for (const tool of tools) console.log(`  ${foreign.includes(tool) ? "!!" : "  "} ${tool}`);

      if (foreign.length > 0) {
        console.log(`\nNot the platform'"'"'s: ${foreign.join(", ")} - name them in DENY in builder.mjs and run this again.`);
        process.exitCode = 1;
      } else {
        console.log("\nOnly the platform'"'"'s MCP tools. The allowlist decides which of them a build may call.");
      }
    }
  });

  child.on("close", () => {
    fs.rmSync(work, { recursive: true, force: true });
    if (!seen) {
      console.log("The CLI ended before it said which tools it loaded.");
      process.exitCode = 1;
    }
  });
'
