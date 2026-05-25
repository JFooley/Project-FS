using SFML.Graphics;
using SFML.System;
using UI_space;

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

    // Winner
    public const int Drawn = 0;
    public const int PlayerA = 1;
    public const int PlayerB = 2;

    public static int match_winner = 0;
    public static int round_winner = 0;

    public static int battle_state = Intro;
    public static int battle_mode = Versus;
    
    private Sprite fight_logo;
    private Sprite timesup_logo;
    private Sprite KO_logo;
    private Sprite fade90;

    public WGBattle() {
        fight_logo = new Sprite(Data.textures["typography:fight"]);
        timesup_logo = new Sprite(Data.textures["typography:timesup"]);
        KO_logo = new Sprite(Data.textures["typography:ko"]); 
        fade90 = new Sprite(Data.textures["screens:90fade"]) {Color = Color.White};
    }

    public override void Render() {
        Program.stage?.Update();

        switch (WGBattle.battle_state) {
            case Intro:
                this.fade90.Position = new Vector2f(Camera.X - Config.RenderWidth/2, Camera.Y - Config.RenderHeight/2);
                this.fade90.Color = new Color(255, 255, 255, (byte) Math.Max(0, this.fade90.Color.A - 15));
                Program.window.Draw(fade90);

                Program.stage?.SetMusicVolume();
                Program.stage?.StopRoundTime();
                Program.stage?.ResetTimer();
                if (Program.stage.character_A.state.not_busy && Program.stage.character_B.state.not_busy && fade90.Color.A == 0) 
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
                    Accessibility.Speak("RoundFight", TTSRequisition.TEXT, true, S("fight!"));
                }
                else if (Program.stage.CheckTimer(1)) 
                    UI.DrawText(S("ready?"), 0, -30, TTS: true, TTS_id: "RoundReady", priority: true, spacing: Config.spacing_medium, textureName: "default medium white");
                else 
                    UI.DrawText(S("round", " ", Program.stage.round.ToString()), 0, -30, TTS: true, TTS_id: "RoundStart", priority: true, spacing: Config.spacing_medium, textureName: "default medium white");

                break;

            case Battling: // Durante a batalha
                if (Program.stage.CheckRoundEnd()) {
                    WGBattle.battle_state = RoundEnd;
                    Program.stage.StopRoundTime();
                    Program.stage.ResetTimer();
                }
                break;

            case RoundEnd: // Fim de round
                if (Program.stage.CheckTimer(6)) {
                    Program.stage.LockPlayers();
                    Program.stage.ResetTimer();
                    if (Program.stage.CheckMatchEnd()) {
                        WGBattle.battle_state = MatchEnd;
                    } else {
                        WGBattle.battle_state = RoundStart;
                        Program.stage.ResetPlayers();
                    }

                } else if (Program.stage.CheckTimer(3)) {
                    if (WGBattle.round_winner == Drawn) {
                        UI.DrawText(S("draw"), 0, -30, TTS: true, TTS_id: "RoundResult", priority: true, spacing: Config.spacing_medium, textureName: "default medium white");
                    } else {
                        UI.DrawText(S(WGBattle.round_winner == 1 ? "player 1" : "player 2", " ", "wins"), 0, -30, TTS: true, TTS_id: "RoundResult", priority: true, spacing: Config.spacing_medium, textureName: "default medium white");
                    }

                } else if (!Program.stage.CheckTimer(3)) {
                    if (Program.stage.character_A.life_points.X <= 0 || Program.stage.character_B.life_points.X <= 0) {
                        KO_logo.Position = new Vector2f(Camera.X - 75, Camera.Y - 54);
                        Program.window.Draw(KO_logo);
                        Accessibility.Speak("RoundEndLogo", TTSRequisition.TEXT, true, S("K.O."));
                    } else {
                        timesup_logo.Position = new Vector2f(Camera.X - 131, Camera.Y - 55);
                        Program.window.Draw(timesup_logo);
                        Accessibility.Speak("RoundEndLogo", TTSRequisition.TEXT, true, S("time's up"));
                    }
                } 
                break;

            case MatchEnd: // Fim da partida
                this.fade90.Position = new Vector2f(Camera.X - Config.RenderWidth/2, Camera.Y - Config.RenderHeight/2);
                this.fade90.Color = new Color(255, 255, 255, (byte) Math.Min(255, this.fade90.Color.A + 15));
                Program.window.Draw(fade90);

                if (this.fade90.Color.A == 255) {
                    Camera.Reset();
                    Program.stage?.ResetMatch();
                    Program.stage?.ResetPlayers(force: true, total_reset: true);
                    WGBattle.battle_state = Intro;
                    Program.ChangeState(Program.PostBattle);   
                }
                break;
        } 
    }
}