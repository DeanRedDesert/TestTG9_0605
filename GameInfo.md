# Game Info Json File

This is a description of all the options and fields you can edit in the GameInfo.json file.

---

**GameName:** (**string**) The name of the game. This is used to generate the registry files in the _./Game/Registries_ folder.

### UgpGameId
This UGP ID information used to generate the registry files in the _./Game/Registries_ folder and also the _./Documentation/GameInfo.txt_ file.

**GameId:**  (**string**) The id of the game. eg "X0000".\
**Version:**  (**integer**) The external version of the game.

### AscentGameId
This Ascent ID information used to generate the registry files in the *./Game/Registries* folder.\
The ProgressiveTheme is also used to generate the *./Documentation/GameInfo.txt* file.

**StudioId:** (**string**) The ID of the studio. "R" is Midas, "H" is Beijing.\
**PlatformId:** (**string**) The ID of the platform. "1" is Dual, "9" is Curve.\
**FamilyNumber:** (**string**)" The family number. Ascent is "020".\
**SapThemeId:** (**string**) The SAP game id. e.g. "TMP".\
**GameRevision:** (**integer**) The external revision of the game.\
**ProgressiveTheme:** (**string**) The name of the progressive. e.g. "BuBuGau".\
**StartingPayVarIndex:** (**integer**) The starting ID of the paytables. Should be greater than 50000.\
**GameSetSharedInterfaceId:** (**string**) A game set shared interface guid, if required, otherwise leave as an empty string. e.g. "6f3ba173-abce-424a-9b45-35eef817fe8c".

### EgmConfiguration
Information used for configuring the game on the EGM.\
This data is used to generate the registry files in the *./Game/Registries* folder.

**GambleOptions:** (**string**) The gamble option the game supports:

>- "None"
>- "Trumps"
>- "BeatTheDealer"
>- "Custom"

**CustomGambleName:** (**string**) If GambleOptions is "Custom", then provide the gamble name.  
**VideoTopper:** (**string**) The video topper option the game supports:
>- "None"
>- "Embedded": A video file in EGMResources folder is played by the foundation.
>- "Unity": Unity is set up to provide content for the video topper.

**BankSyncId:** (**string**)" The ID used for the Bank Sync protocol.\
**SupportedCultures:** (**list of string**) A list of languages supported by the game. e.g. "en-US", "es-ES"\
**SupportsUtilityMode:** (**true|false**) Does the game support utility mode? Only Macau requires this.\
**SupportsCreditPlayoff:** (**true|false**) Does the game support credit play off?\
**SupportsCashoutServiceButtons:** (**true|false**) Does the game support touch screen versions of cashout and service?\
**MaxDenoms:** (**integer** ) The maximum number of denoms that can be enabled. If 0, the number will be automatically determined.\
**UseSasProgressives:**  (**true|false**) Is the game setup to use a SAS progressive controller?

### Game Definition
Information used to automatically populate game submission documents via the *./Documentation/GameInfo.txt* file.

**AnteBetType:** (**string**) The type of ante bet.
>- "None"
>- "LinesPlusAnte"
>- "LinesAndCredits"
>- "MultiwayPlusAnte"
>- "Lines"
>- "Multiway"
>- "CreditsOnly"

**AnteBetTheme:** (**string**) The ante bet theme if applicable. Can be empty.\
**ReelCount:** (**integer**) The number of reels on the base game.\
**Theme:** (**string**) The overall game theme.

### PidConfiguration
Information used for customising the code generation of the *./Game/Assets/GleLogic/GamePid.cs* file.

**PrizeOverrides:** (**dictionary of string -> string**) Specify string pairs to rename a generic PID prize name to a theme specific name.\
For example:

    "PidConfiguration": {
      "PrizeOverrides": {
        "M1": "JET",
        "M2": "TRUCK",
        "M3": "BOAT",
        "M4": "CAR"
      }
    }

# Default GameInfo.json File

    {
        "GameName": "No Name",
        "UgpGameId": {
            "GameId": "X0000",
            "Version": 0
        },
        "AscentGameId": {
            "FamilyNumber": "020",
            "GameRevision": 0,
            "PlatformId": "001",
            "ProgressiveTheme": "ProgTheme",
            "SapThemeId": "TMP",
            "StartingPayVarIndex": 50000,
            "StudioId": "R",
            "GameSetSharedInterfaceId": ""
        },
        "EgmConfiguration": {
            "BankSyncId": "ProgThemeBank",
            "CustomGambleName": "",
            "GambleOptions": "Trumps",
            "MaxDenoms": 0,
            "SupportedCultures": [
                "en-US"
            ],
            "SupportsCashoutServiceButtons": false,
            "SupportsCreditPlayoff": true,
            "SupportsUtilityMode": false,
            "UseSasProgressives": false,
            "VideoTopper": "Unity"
        },
        "GameDefinition": {
            "AnteBetTheme": "",
            "AnteBetType": "None",
            "ReelCount": 0,
            "Theme": "None"
        },
        "PidConfiguration": {
            "PrizeOverrides": {
            }
        }
    }