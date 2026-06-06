## Context

El catálogo actual tiene 66 items. Se agregan 25 nuevos + 6 remeras específicas. La remera "67" usa el número gigante y texto "sixseven" como meme comunitario. La remera México reutiliza el PNG de Zapdos existente como águila del escudo.

## Goals / Non-Goals

**Goals:**
- 5 peinados CSS nuevos con estilos visualmente distintos
- 6 remeras CSS: 67 (número + texto gigante), México (Zapdos sobre verde/blanco/rojo), EE.UU. (barras y estrellas), Canadá (hoja roja), Corea (taeguk), Inglaterra (cruz roja)
- 5 gorros: bruja (SVG puntiagudo), galera (CSS), beanie (CSS), pirata (SVG tricornio), orejas gato (SVG)
- 5 anteojos: redondos (CSS), corazón (SVG), estrella (SVG), cibernético (SVG scanner), diva (CSS)
- 5 items mano: tambor (SVG), bastón (SVG), antorcha (SVG), escudo (CSS), flores (CSS)

**Non-Goals:**
- No se modifican accesorios existentes
- No se agregan nuevas dependencias

## Decisions

### 1. Peinados: todos CSS

Mismo patrón que los 9 existentes. `.hair-style-10` a `.hair-style-14` con `clip-path`, `box-shadow`, y `background` para simular volumen y forma.

### 2. Remeras: CSS con pseudo-elementos

Siguen el patrón `.torso.shirt-*` con `::before`/`::after` para números y escudos. La remera México usa el PNG existente `imgs/Pokemon/zapdos.png` como `::before` con `background-image: url(...)`.

### 3. Gorros y anteojos: SVG inline para los complejos, CSS para los simples

SVG: bruja (puntiagudo con gradiente), pirata (tricornio), orejas gato, corazón, estrella, cibernético.
CSS: galera (bordes + gradiente), beanie (pliegues), redondos, diva.

### 4. Items de mano: SVG para los detallados, CSS para los simples

SVG: tambor (cilindro + parche), bastón (empuñadura curva), antorcha (llama).
CSS: escudo, flores.

## Risks / Trade-offs

- **Zapdos PNG como águila de México** → la imagen 108x108 se escala. Puede verse pixelada si el PNG es chico. Alternativa: SVG de águila, pero sería más trabajo.
- **26 items nuevos** → carga de CSS/HTML. El archivo avatar.css ya tiene 965 líneas, llegará a ~1200. Rendimiento no debería verse afectado (solo carga inicial del CSS).
