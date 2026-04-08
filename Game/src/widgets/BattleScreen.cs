using SFML.Graphics;
using SFML.System;
using UI_space;
using Language_space;
using System.CodeDom;

class WGBattle : Widget {
    // Battle States
    public const int Intro = 0;
    public const int RoundStart = 1;
    public const int Battling = 2;
    public const int RoundEnd = 3;
    public const int MatchEnd = 4;

    // Battle modes
    public const int Versus = 0;
    public const int VersusBot = 1;
    public const int Training = 2;

    public static int battle_state = Intro;
    public static int battle_mode = Versus;
    Sprite fight_logo = new Sprite(Data.textures["typography:fight"]);
    Sprite timesup_logo = new Sprite(Data.textures["typography:timesup"]);
    Sprite KO_logo = new Sprite(Data.textures["typography:ko"]); 

    public override void Render() {
        Program.stage?.Update();

        switch (WGBattle.battle_state) {
            case Intro:
                Program.stage.SetMusicVolume();
                Program.stage.StopRoundTime();
                Program.stage.ResetTimer();
                if (Program.stage.character_A.current_state == "Idle" && Program.stage.character_B.current_state == "Idle") 
                    WGBattle.battle_state = RoundStart;
                break;

            case RoundStart: // Inicia a round
                if (Program.stage.CheckTimer(3)) {
                    Program.stage.ResetRoundTime();
                    Program.stage.StartRoundTime();
                    Program.stage.ReleasePlayers();
                    WGBattle.battle_state = Battling;
                }
                else if (Program.stage.CheckTimer(2)) {
                    fight_logo.Position = new Vector2f(Camera.X - 89, Camera.Y - 54);
                    Program.window.Draw(fight_logo);
                }
                else if (Program.stage.CheckTimer(1)) UI.DrawText(Language.GetText("Ready")+"?", 0, -30, spacing: Config.spacing_medium, textureName: "default medium white");
                else UI.DrawText(Language.GetText("Round") + " " + Program.stage.round, 0, -30, spacing: Config.spacing_medium, textureName: "default medium white");

                break;

            case Battling: // Durante a batalha
                if (Program.stage.CheckRoundEnd()) {
                    WGBattle.battle_state = RoundEnd;
                    Program.stage.StopRoundTime();
                    Program.stage.ResetTimer();
                }
                break;

            case RoundEnd: // Fim de round
                if (!Program.stage.CheckTimer(3)) {
                    if (Program.stage.character_A.life_points.X <= 0 || Program.stage.character_B.life_points.X <= 0) {
                        KO_logo.Position = new Vector2f(Camera.X - 75, Camera.Y - 54);
                        Program.window.Draw(KO_logo);
                    } else {
                        timesup_logo.Position = new Vector2f(Camera.X - 131, Camera.Y - 55);
                        Program.window.Draw(timesup_logo);
                    }
                }
                if (Program.stage.CheckTimer(4)) {
                    Program.stage.LockPlayers();
                    Program.stage.ResetTimer();
                    if (Program.stage.CheckMatchEnd()) {
                        WGBattle.battle_state = MatchEnd;
                    } else {
                        WGBattle.battle_state = RoundStart;
                        Program.stage.ResetPlayers();
                    }
                }
                break;

            case MatchEnd: // Fim da partida
                Camera.Reset();
                Program.stage.ResetMatch();
                Program.stage.ResetPlayers(force: true, total_reset: true);

                WGBattle.battle_state = Intro;
                Program.ChangeState(Program.PostBattle);
                break;
        } 
    }
}