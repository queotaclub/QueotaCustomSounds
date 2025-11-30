# QueotaCustomSounds
Custom CounterStrikeSharp plugin for CS2 that picks a random configurable sound and plays it at the end of each round for each player.

❗ Only works with sounds published on the CS2 workshop, see how to upload your own below ❗

## Installation
- Install dependency addons:
   - [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp) 
   - [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)
   - [MultiAddonManager](https://github.com/Source2ZE/MultiAddonManager)
- Download a release from [QueotaCustomSounds](https://github.com/QUEOTA/QueotaCustomSounds/releases) *(replace with actual repo if needed)*
- Unzip and place in `csgo/addons/counterstrikesharp/plugins`
- Add all your soundpacks from the CS2 workshop to your MultiAddonManager configuration
- Add all the paths of the sound files from the sound packs to the plugin config located at `csgo/addons/counterstrikesharp/configs/plugins/QueotaCustomSounds/QueotaCustomSounds.json` (generated automatically after the plugin is first loaded, a config example named `QueotaCustomSounds.example.json` is provided in this repo)


## Uploading your own sound pack to workshop
You will need [python](https://www.python.org/downloads/windows/) (Download `Windows installer (64-bit)`) for this to work.

1. Open the CS2 workshop tools.
2. Add a new addon, click create.
3. Right click your addon and click `Explore Content Folder`.
4. Here we can place any resource file but to follow a best practise follow this structure.
5. Navigate to the `sounds` folder. You're now in `<addon name>/sounds`.
6. Add a folder for your sounds, for example `my_sound_pack`. You're now in `<addon name>/sounds/my_sound_pack`.
7. Add your sound files here. Example of one of the files: `<addon name>/sounds/my_sound_pack/sound1.wav`.
8. Go back to you addon root folder, `<addon name>`.
9. Create a python file named `generate_description.py` in this folder.
10. Paste the following code into the file: (replace .mp3 (at the bottom) with the file extension of your sound files if needed) 
```python
import os

OUTPUT_FILE = "created_sounds.vsndevts"

def append_to_file(file_path):
    # file_path: sounds/foo/bar/file.wav

    # nome sem extensão
    basename = os.path.splitext(os.path.basename(file_path))[0]

    # remove prefixo "sounds/" do path completo
    rel = os.path.splitext(file_path)[0]  # remove .wav
    rel = rel.replace("\\", "/")          # normaliza separadores
    rel = rel[2:] + ".vsnd" if rel.startswith("./") else rel + ".vsnd"

    # cria "name" usando pontos
    name = basename
    name = name.replace("\\", ".").replace("/", ".")

    print(name)

    s = (
        f"\"{name}\"\n"
        "{\n"
        f"    \"{name}\" = \n"
        "    {\n"
        "        base = \"amb.looping.stereo.base\"\n"
        "        volume = 1\n"
        "        pitch = 1\n"
        f"        vsnd_files_track_01 = \"{rel}\"\n"
        "    }\n"
        "}\n\n"
    )

    with open(OUTPUT_FILE, "a", encoding="utf-8") as f:
        f.write(s)


for root, dirs, files in os.walk("sounds"):
    for file in files:
        if file.endswith(".wav"):
            append_to_file(os.path.join(root, file))
  ```
11. Open a terminal window in this folder. You can do this by typing `cmd` in the address bar (not the search bar) of the file explorer and pressing enter.
12. Run the following command: `python generate_description.py` and go back to your file explorer.
13. Move the generated `created_sounds.vsndevts` file inside the `soundevents` folder. 
14. Go back to the workshop tools, select your addon and click the `Edit Addon Map` button.
15. On the top right, click `Tools` and select `Asset Browser`.
16. In the filters, enter the name of the folder inside the sounds folder, in our example `my_sound_pack` of `<addon name>/sounds/my_sound_pack/sound1.wav`.
17. Use shift to select all the files and press `Right Click` on one of them.
18. Select `Full Recompile`
19. Now left-click on any one asset with the left mouse button. 
20. In the preview window there will be a link to `Game File` - click to open in explorer.
21. In this file there should be vsnd_c formatted music files stored for all of your sound files.
22. Now submit the addon to the workshop. 
23. Once accepted use its ID for MultiAddonManager and use the paths of the sound files in the plugin config.
    (Example of a path: `sounds/my_sound_pack/sound1.vsnd`, note the `.vsnd` extension)