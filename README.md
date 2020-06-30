# UnityDarkenThemer

Keep in mind this package is experimental and there are some issues.

[![UnityDarkenThemer](http://img.youtube.com/vi/8VbNQfeyJmI/0.jpg)](http://www.youtube.com/watch?v=8VbNQfeyJmI "UnityDarkenThemer")

## Setup:
1. In Unity click Window -> Package Manager -> click plus sign -> Add package from git URL:
```
https://github.com/kamyker/UnityDarkenThemer.git
```
2. Click Tools -> Darken -> Create Textures
This option saves and inverts colors of all editor textures to `Assets\Editor Default Resources\Icons`. It also creates ScriptableObject settings in `Assets\UnityDarkenThemer\UnityDarkenThemerSettings.asset`. If you are lucky it will also initialize for the first time and invert all colors.
3. If 2. wasn't enough restart Unity.
4. When after opening any window (for ex. Package Manager) colors are not inverted click "Tools -> Darken -> Quick Fix".
5. When after opening any window icons are not inverted repeat step 2.

## Uninstallation:
1. Remove package from Package Manager.
2. Remove "Library\Style.catalog" file as that's where Unity caches style sheets.
3. Remove "Assets\Editor Default Resources\Icons" folder.
4. Restart Unity.

## Theming
By default inverted colors have 5% less blue color to change it:
1. Select "Assets\UnityDarkenThemer\UnityDarkenThemerSettings" scriptable object>
2. Modify "Additive Colors" array 
3. Restart Unity
4. Repeat 2-3 until you find your theme
5. Remove `Assets\Editor Default Resources\Icons` folder and generate textures again with "Tools -> Darken -> Create Textures"
6. GO CRAZY:

![UnityTheming](https://i.gyazo.com/4b08eb4e58bc5fb3d80d523135d59502.png)

Just kidding, purpose of this image is to see how gradient defined in step 2. matches other colors and panels:
![UnityThemingGradient](https://i.gyazo.com/94c5bb0cd5e592b1c13db5367b1476dc.png)
