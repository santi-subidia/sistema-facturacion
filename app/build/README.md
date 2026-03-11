# Icono de la aplicación

Agregar aquí el archivo `icon.ico` (256x256 o mayor) para el instalador de Windows.

Para generar el .ico desde un PNG se puede usar:
- https://www.icoconverter.com/ (online)
- `magick convert icon.png -define icon:auto-resize=256,128,64,32,16 icon.ico` (ImageMagick)

El `electron-builder.yml` busca `build/icon.ico`.
Si no hay icono, electron-builder usa el ícono default de Electron.
