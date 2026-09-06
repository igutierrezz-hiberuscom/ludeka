import fs from "fs";
import path from "path";

const skillsDir = path.resolve(".agents/skills");

function updateLanguageContract(filePath) {
  let content = fs.readFileSync(filePath, "utf8");
  let modified = false;

  // Pattern 1: Multi-line contract block
  const oldContract1 = /Generated technical artifacts default to English\.[^#]*(?=##|\n\n##|$)/g;
  const newContract1 = `Generated technical artifacts MUST BE IN SPANISH (CASTELLANO). All proposals, specifications, designs, tasks, reports, reviews, user-facing text, and explanations must be written in Spanish (castellano). Code identifiers (C# classes, methods, variables) may follow English naming conventions, but all markdown artifacts, comments, and communications must be in Spanish.`;

  if (content.includes("Generated technical artifacts default to English")) {
    // Replace the specific sentences
    content = content.replace(
      /Generated technical artifacts default to English\. Do not inherit the user's conversational language or the active persona's regional voice for SDD artifacts unless the user explicitly requests that artifact language or the project convention requires it\./g,
      "Generated technical artifacts MUST BE IN SPANISH (CASTELLANO). All artifacts, proposals, specs, designs, and tasks must be written in Spanish (castellano) as mandated by project convention."
    );
    content = content.replace(
      /If technical artifacts are explicitly requested in another language, use a neutral\/professional register unless the user explicitly requests a different tone or regional variant\./g,
      "Use a natural, clear, and professional Spanish register for all generated documents."
    );
    content = content.replace(
      /- Generated technical artifacts default to English\. If technical artifacts are explicitly requested in another language, use a neutral\/professional register\./g,
      "- Generated technical artifacts MUST BE IN SPANISH (CASTELLANO). Use a clear and professional Spanish register."
    );
    modified = true;
  }

  if (modified) {
    fs.writeFileSync(filePath, content, "utf8");
    console.log(`✓ Updated language contract in: ${path.relative(process.cwd(), filePath)}`);
  }
}

function walkDir(dir) {
  const files = fs.readdirSync(dir);
  for (const file of files) {
    const fullPath = path.join(dir, file);
    const stat = fs.statSync(fullPath);
    if (stat.isDirectory()) {
      walkDir(fullPath);
    } else if (file.endsWith(".md")) {
      updateLanguageContract(fullPath);
    }
  }
}

walkDir(skillsDir);
