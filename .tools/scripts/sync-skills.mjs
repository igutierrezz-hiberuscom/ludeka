import fs from "fs";
import path from "path";

const AUTOSKILLS = [
  "frontend-design",
  "accessibility",
  "web-perf",
  "dotnet-best-practices",
  "dotnet-design-pattern-review",
  "csharp-async",
  "csharp-xunit",
  "aspnet-core",
  "aspnet-minimal-api-openapi",
  "fluentui-blazor",
  "tailwind-css-patterns"
];

const GENTLE_SDD_SKILLS = [
  "sdd-init",
  "sdd-explore",
  "sdd-research",
  "sdd-propose",
  "sdd-spec",
  "sdd-design",
  "sdd-tasks",
  "sdd-apply",
  "sdd-verify",
  "sdd-archive",
  "cognitive-doc-design",
  "work-unit-commits"
];

const targetSkillsDir = path.resolve(".agents/skills");
if (!fs.existsSync(targetSkillsDir)) {
  fs.mkdirSync(targetSkillsDir, { recursive: true });
}

async function fetchJson(url) {
  const res = await fetch(url, { headers: { "User-Agent": "Ludeka-Skills-Sync" } });
  if (!res.ok) throw new Error(`Failed to fetch ${url}: ${res.statusText}`);
  return await res.json();
}

async function downloadFile(url, dest) {
  const dir = path.dirname(dest);
  if (!fs.existsSync(dir)) fs.mkdirSync(dir, { recursive: true });
  const res = await fetch(url, { headers: { "User-Agent": "Ludeka-Skills-Sync" } });
  if (!res.ok) throw new Error(`HTTP ${res.status} downloading ${url}`);
  const text = await res.text();
  fs.writeFileSync(dest, text, "utf8");
}

async function syncAutoskills() {
  console.log("=== Sincronizando skills de midudev/autoskills ===");
  for (const skill of AUTOSKILLS) {
    try {
      const url = `https://api.github.com/repos/midudev/autoskills/contents/packages/autoskills/skills-registry/${skill}`;
      const items = await fetchJson(url);
      const skillDest = path.join(targetSkillsDir, skill);
      for (const item of items) {
        if (item.type === "file") {
          await downloadFile(item.download_url, path.join(skillDest, item.name));
        } else if (item.type === "dir") {
          const subItems = await fetchJson(item.url);
          for (const sub of subItems) {
            if (sub.type === "file") {
              await downloadFile(sub.download_url, path.join(skillDest, item.name, sub.name));
            }
          }
        }
      }
      console.log(`✓ [autoskills] ${skill}`);
    } catch (err) {
      console.error(`✗ Error descargando ${skill}:`, err.message);
    }
  }
}

async function syncGentleSdd() {
  console.log("=== Sincronizando skills SDD de Gentleman-Programming/gentle-ai ===");
  for (const skill of GENTLE_SDD_SKILLS) {
    try {
      const url = `https://api.github.com/repos/Gentleman-Programming/gentle-ai/contents/internal/assets/skills/${skill}`;
      const items = await fetchJson(url);
      const skillDest = path.join(targetSkillsDir, skill);
      for (const item of items) {
        if (item.type === "file") {
          await downloadFile(item.download_url, path.join(skillDest, item.name));
        } else if (item.type === "dir") {
          const subItems = await fetchJson(item.url);
          for (const sub of subItems) {
            if (sub.type === "file") {
              await downloadFile(sub.download_url, path.join(skillDest, item.name, sub.name));
            }
          }
        }
      }
      console.log(`✓ [gentle-sdd] ${skill}`);
    } catch (err) {
      console.error(`✗ Error descargando ${skill}:`, err.message);
    }
  }
}

async function main() {
  await syncAutoskills();
  await syncGentleSdd();
  console.log("\n¡Todas las skills han sido sincronizadas en .agents/skills/!");
}

main();
