#!/usr/bin/env node
/**
 * convert-hero-images.mjs — INC-35 (Decisión 10 del diseño).
 *
 * Convierte las fotos de ambiente e ilustración del hero de la portada:
 *   - redimensiona a 1600 px de ancho (sin ampliar),
 *   - genera AVIF (q≈50) y WebP (q≈72) junto al JPEG de fallback (q≈75),
 *   - imprime el peso de cada salida y avisa si supera los 200 KB de presupuesto,
 *   - re-comprime el JPEG en sitio SOLO si aún supera el presupuesto (una re-ejecución
 *     con el JPEG ya optimizado no vuelve a comprimir, para evitar deriva generacional).
 *
 * Uso (sharp se instala fuera del repositorio para no ensuciarlo):
 *   npm i --no-save sharp   (en un directorio temporal)
 *   NODE_PATH=<dir>/node_modules node scripts/convert-hero-images.mjs
 *
 * Si alguna salida superase 200 KB incluso a la calidad mínima, el script avisa y sale
 * con código 1 (el presupuesto de peso del hero es requisito de spec, no recomendación).
 */

import { createRequire } from 'node:module';
import { readdirSync, readFileSync, statSync, writeFileSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const require = createRequire(import.meta.url);
let sharp;
try {
  sharp = require('sharp');
} catch {
  console.error(
    'No se encontró "sharp". Instálalo en un directorio temporal y ejecuta con NODE_PATH apuntando a su node_modules:\n' +
    '  npm i --no-save sharp\n' +
    '  NODE_PATH=<dir-temporal>/node_modules node scripts/convert-hero-images.mjs'
  );
  process.exit(1);
}

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const HOME_DIR = path.resolve(__dirname, '../src/Ludeka.Web/wwwroot/images/home');

const TARGET_WIDTH = 1600;
const BUDGET_BYTES = 200 * 1024;

// Calidad inicial por formato (≈ de la Decisión 10) y escalones de descenso hasta caber
// en el presupuesto de peso de la spec (los pesos reales se imprimen al final).
const QUALIDADES = {
  avif: [50, 44, 38, 32],
  webp: [72, 64, 56, 48, 40],
  jpg: [75, 68, 60, 52, 44, 36],
};

const formatKB = (bytes) => `${(bytes / 1024).toFixed(1)} KB`;

async function encodeConPresupuesto(pipeline, formato, calidades) {
  let ultimo = null;
  for (const calidad of calidades) {
    const clon = pipeline.clone();
    const buffer = formato === 'avif'
      ? await clon.avif({ quality: calidad, effort: 4 }).toBuffer()
      : formato === 'webp'
        ? await clon.webp({ quality: calidad }).toBuffer()
        : await clon.jpeg({ quality: calidad, progressive: true, mozjpeg: true }).toBuffer();

    ultimo = { buffer, calidad };
    if (buffer.length <= BUDGET_BYTES) {
      return ultimo;
    }
  }
  return ultimo; // se reporta como aviso; nada se descarta en silencio
}

async function convertir(archivo) {
  const nombreBase = path.basename(archivo, '.jpg');
  // La fuente se lee a memoria UNA vez: todos los formatos se derivan del buffer y
  // así el JPEG puede re-escribirse en sitio sin compartir descriptores con libvips.
  const fuente = readFileSync(archivo);
  const meta = await sharp(fuente).metadata();
  const escala = Math.min(1, TARGET_WIDTH / meta.width);
  const ancho = Math.round(meta.width * escala);
  const alto = Math.round(meta.height * escala);
  const pipeline = sharp(fuente).resize({ width: TARGET_WIDTH, withoutEnlargement: true });

  console.log(`\n▶ ${nombreBase} (fuente ${meta.width}x${meta.height}, ${formatKB(statSync(archivo).size)}) → salida ${ancho}x${alto}`);

  const resultados = [];

  // JPEG de fallback: se re-comprime solo si el JPEG actual aún excede el presupuesto.
  const pesoActualJpg = statSync(archivo).size;
  if (pesoActualJpg > BUDGET_BYTES) {
    const { buffer, calidad } = await encodeConPresupuesto(pipeline, 'jpg', QUALIDADES.jpg);
    writeFileSync(archivo, buffer);
    resultados.push({ formato: 'jpg', archivo, bytes: buffer.length, calidad });
  } else {
    console.log(`   jpg ya dentro de presupuesto (${formatKB(pesoActualJpg)}): no se re-comprime (evita deriva generacional)`);
    resultados.push({ archivo, bytes: pesoActualJpg, calidad: null, omitido: true });
  }

  for (const formato of ['avif', 'webp']) {
    const destino = path.join(HOME_DIR, `${nombreBase}.${formato}`);
    const { buffer, calidad } = await encodeConPresupuesto(pipeline, formato, QUALIDADES[formato]);
    writeFileSync(destino, buffer);
    resultados.push({ formato, archivo: destino, bytes: buffer.length, calidad });
  }

  for (const r of resultados) {
    if (r.omitado) {
      console.log(`   ${path.basename(r.archivo)}: ${formatKB(r.bytes)} (sin cambios)`);
      continue;
    }
    const sobrePresupuesto = r.bytes > BUDGET_BYTES ? '  ⚠ SUPERIOR A 200 KB' : '';
    console.log(`   ${path.basename(r.archivo)}: ${formatKB(r.bytes)} (q=${r.calidad})${sobrePresupuesto}`);
  }

  return resultados.every((r) => r.bytes <= BUDGET_BYTES);
}

async function main() {
  const fuentes = readdirSync(HOME_DIR)
    .filter((f) => f.startsWith('hero-') && f.endsWith('.jpg'))
    .sort();

  if (fuentes.length === 0) {
    console.error(`No hay fuentes hero-*.jpg en ${HOME_DIR}`);
    process.exit(1);
  }

  console.log(`Conversión del hero (INC-35) — ${fuentes.length} fuente(s), ancho ${TARGET_WIDTH}px, presupuesto ${formatKB(BUDGET_BYTES)}`);

  let dentroDePresupuesto = true;
  for (const fuente of fuentes) {
    const ok = await convertir(path.join(HOME_DIR, fuente));
    dentroDePresupuesto = dentroDePresupuesto && ok;
  }

  console.log(`\nResumen: ${dentroDePresupuesto ? 'todas las salidas dentro del presupuesto de 200 KB' : 'HAY SALIDAS POR ENCIMA DE 200 KB — revisar arriba'}`);
  process.exit(dentroDePresupuesto ? 0 : 1);
}

main();
