<div align="center">

# ⭐ ABYSSAL STARS ⭐

### *Shoot 'em up vertical · Bullet Hell sincronizado con la música*

Pilota tu nave a través del abismo estelar mientras las oleadas enemigas nacen **al ritmo de la banda sonora**. Esquiva, enfoca, bombardea y sobrevive.

<br>

<!-- 📸 Reemplaza la ruta por tu captura vertical (9:16). Ancho recomendado ~270-320 px. -->
<!-- <img src="docs/screenshot.png" alt="Abyssal Stars gameplay" width="300"> -->

![Unity](https://img.shields.io/badge/Unity_6-URP-000000?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Windows](https://img.shields.io/badge/PC-Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white)
![Android](https://img.shields.io/badge/Android-API_26+-3DDC84?style=for-the-badge&logo=android&logoColor=white)

![Género](https://img.shields.io/badge/Género-Bullet_Hell-ff2e63?style=flat-square)
![Resolución](https://img.shields.io/badge/Resolución-9%3A16_vertical-8a2be2?style=flat-square)
![Niveles](https://img.shields.io/badge/Niveles-3_+_Menú-08d9d6?style=flat-square)
![Estado](https://img.shields.io/badge/Estado-En_desarrollo-yellow?style=flat-square)

</div>

---

## 🌌 Sobre el juego

**Abyssal Stars** es un *bullet hell* vertical cuyo sello distintivo es su **sistema de spawn reactivo al audio**: un analizador FFT en tiempo real detecta los *beats* de la canción y hace aparecer a los enemigos en sincronía con las frecuencias graves, medias y agudas de la pista. Cada nivel culmina con un **jefe multifase**, con patrones de ataque y movimiento totalmente configurables desde el editor.

<div align="center">

*Estética ciberpunk espacial · pixel art · acentos cian sobre el vacío*

</div>

---

## 📐 Pantalla y resolución

> [!IMPORTANT]
> **Abyssal Stars está diseñado para el formato clásico vertical 9:16** (orientación retrato), el estándar de los shoot 'em up de tipo *danmaku*. Es la resolución en la que la acción se ve y se disfruta mejor.

La interfaz usa un sistema de **anchors adaptativos**, por lo que el juego **se ajusta a cualquier resolución** (distintos móviles, ventanas de PC, etc.) sin romper el HUD ni los menús. Aun así, para la mejor experiencia se recomienda jugar en **9:16 vertical**.

| | |
|:--|:--|
| **Relación de aspecto objetivo** | 9:16 (vertical / retrato) |
| **Resoluciones ideales** | 1080 × 1920 · 720 × 1280 |
| **Adaptabilidad** | Anchors flexibles + límites de juego calculados desde la cámara |
| **PC** | Se recomienda ejecutar en una ventana vertical 9:16 |

---

## 🎮 Versiones disponibles

El proyecto se distribuye en dos ramas principales, según la plataforma:

<table>
<tr>
<th width="50%">🖥️ &nbsp; v0.3.0 — <em>PC Edition</em></th>
<th width="50%">📱 &nbsp; Android — <em>Mobile Edition</em></th>
</tr>
<tr valign="top">
<td>

**Rama:** `release/v0.3.0`

La versión **completa para escritorio**, tal como se documenta en el GDD. Pensada para teclado y mando.

- ⌨️ Controles de **teclado remapeables** (WASD, Espacio, B, Shift, ESC)
- 🎯 Disparo manual y **modo foco** con Shift
- 💣 Sistema de bombas con tecla dedicada
- 🧩 Menú de Configuración completo (VSync, FPS, remapeo de teclas…)
- 🖥️ Pensada para ventana vertical 9:16

</td>
<td>

**Rama:** `VersionFinal_Android`

La nueva versión **táctil para móviles**, reconstruida sobre la base de PC con controles nativos y UI adaptativa.

- 👆 **Arrastre táctil de pantalla completa** (estilo *Touhou* móvil): tocas donde sea y la nave sigue tu dedo; la velocidad varía según qué tan rápido arrastras
- 📐 **Límites del mapa adaptativos**: los bordes se calculan desde la cámara, así funcionan en cualquier resolución
- 🔫 **Disparo automático** continuo — sin botón
- 💣 Botón táctil de bomba y de pausa
- ⚙️ Menú de Configuración que **oculta las opciones no aplicables** en móvil

</td>
</tr>
</table>

> [!NOTE]
> La versión Android detecta la plataforma automáticamente (`MobileInputManager.IsMobileActive`). El mismo proyecto corre en PC y en Android intercambiando los controles según el dispositivo.

---

## ✨ Características principales

- 🎵 **Spawn reactivo al audio** — enemigos generados por bandas de frecuencia (graves / medios / agudos / sub-graves) mediante FFT en tiempo real.
- 👑 **Jefes multifase** — fases cíclicas, torretas hijas activables, umbrales de transición por porcentaje de HP y muerte "teatral" con explosiones escalonadas.
- 🌠 **Fondos parallax multicapa** — estrellas continuas, lluvias de meteoritos y planetas únicos, con *pooling* dedicado.
- 🧨 **Sistema de bombas** — limpieza de pantalla de emergencia que afecta el rango final.
- 🏆 **Rangos y bonificaciones** — puntuación con rango (C → S+) y bono *No-Bomb* al terminar el nivel.
- ⚡ **Optimización** — *object pooling* de balas, enemigos y decoración de fondo.
- 🎁 **Power-ups** — Shotgun, Familiares orbitales y vidas extra.

---

## 🕹️ Controles

| Acción | 🖥️ PC | 📱 Android |
|:--|:--|:--|
| **Movimiento** | WASD (remapeable) | Arrastre táctil (pantalla completa) |
| **Modo foco** | Shift + movimiento | Automático (arrastre lento) |
| **Disparo** | Espacio | Automático (continuo) |
| **Bomba** | B | Botón táctil |
| **Pausa** | ESC | Botón táctil |

---

## 🧠 Bajo el capó

<div align="center">

| Sistema | Descripción |
|:--|:--|
| 🎚️ **AudioBeatDetector** | FFT (`GetSpectrumData`, ventana BlackmanHarris) en 4 bandas con crossfade a la música del jefe |
| 🧬 **ScriptableObjects** | Fases, patrones de ataque y de movimiento del jefe, configurables desde el Inspector |
| ♻️ **Object Pooling** | `EnemyPool`, `BulletPool`, `DecorPool` genéricos |
| 💾 **DataManager** | Persistencia de ajustes, progreso y marcador en `PlayerPrefs` (JSON) |
| 📱 **MobileInputManager** | Detección de plataforma + control táctil por arrastre relativo |

</div>

---

## 🚀 Cómo ejecutar

```bash
# 1. Clona el repositorio
git clone https://github.com/DylanEspinoza98/Abyssal_Stars.git

# 2. Ábrelo con Unity 6 (URP)

# 3. Elige tu versión:
#    PC      -> rama  release/v0.3.0
#    Android -> rama  VersionFinal_Android
```

**Resolución:** configura la vista en **9:16 vertical** (p. ej. 1080 × 1920) para la mejor experiencia.
**Escenas** (Build Profiles): `0` MainMenu · `1` Primer_Nivel · `2` Segundo_Nivel · `3` Tercer_Nivel.
**Build Android:** IL2CPP · ARM64 · API mínima 26.

---

## 🎨 Assets & créditos

<details>
<summary><b>Ver assets utilizados</b></summary>

<br>

| Asset | Fuente | Uso |
|:--|:--|:--|
| Foozle — Void MainShip / EnemyFleet / Environment | itch.io | Naves y fondos |
| 2D Space Kit | Unity Asset Store | Elementos espaciales |
| Cyberpunk UI Asset Pack v1.3 | Externo | Botones y HUD |
| Button FX Pack | Externo | SFX de botones |
| Controller Buttons and Thumbstick | Unity Asset Store | Referencias de UI táctil |
| Música | Varios artistas | Banda sonora |

</details>

---

<div align="center">

## 👥 Equipo

**Dylan Espinoza** · **Victor Ugalde**

*Programación de Videojuegos*

<br>

⭐ *Si te gusta el proyecto, deja una estrella en el repo* ⭐

</div>
