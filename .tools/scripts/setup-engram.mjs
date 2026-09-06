import fs from "fs";
import path from "path";
import https from "https";
import { execSync } from "child_process";

const downloadUrl = "https://github.com/Gentleman-Programming/engram/releases/download/v1.20.0/engram_1.20.0_windows_amd64.zip";
const targetDir = path.resolve(".tools/bin");
const zipPath = path.join(targetDir, "engram.zip");

if (!fs.existsSync(targetDir)) {
  fs.mkdirSync(targetDir, { recursive: true });
}

console.log("Downloading engram v1.20.0...");
const file = fs.createWriteStream(zipPath);

https.get(downloadUrl, (response) => {
  if (response.statusCode === 302 || response.statusCode === 301) {
    https.get(response.headers.location, (redirectResponse) => {
      redirectResponse.pipe(file);
      file.on("finish", () => {
        file.close();
        console.log("Extracting engram.zip...");
        execSync(`tar -xf "${zipPath}" -C "${targetDir}"`);
        console.log("Engram extracted successfully to " + targetDir);
        if (fs.existsSync(zipPath)) fs.unlinkSync(zipPath);
      });
    });
  } else {
    response.pipe(file);
    file.on("finish", () => {
      file.close();
      console.log("Extracting engram.zip...");
      execSync(`tar -xf "${zipPath}" -C "${targetDir}"`);
      console.log("Engram extracted successfully to " + targetDir);
      if (fs.existsSync(zipPath)) fs.unlinkSync(zipPath);
    });
  }
}).on("error", (err) => {
  console.error("Download error:", err.message);
});
