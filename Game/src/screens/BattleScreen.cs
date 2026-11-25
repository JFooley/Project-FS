using SFML.Graphics;
using SFML.System;
using UI_space;

class WGBattle : Widget {
    // Battle States
    public enum State {
        Intro,
        RoundStart,
        Battling,
        RoundEnd,
        MatchEnd
    }

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

    public static State battle_state = State.Intro;
    public static int battle_mode = Versus;
    
    private Sprite fade90;

    public WGBattle() {
        fade90 = new Sprite(Data.textures["screens:90fade"]) {Color = Color.White};
    }

    public override void Render() {
        Render(updateStage: true);
    }

    public void Render(bool updateStage) {
        if (updateStage) Program.stage?.Update();
        Program.stage?.Render();
        
        if (Input.Key_down("Start") && WGBattle.battle_state == State.Battling) Program.stage?.Pause();
        if (Stage.training_mode) Program.stage?.TrainingMode();
        if (Stage.pause) Program.stage?.PauseScreen();

        switch (WGBattle.battle_state) {
            case State.Intro:
                this.fade90.Position = new Vector2f(Camera.X - Config.RenderWidth/2, Camera.Y - Config.RenderHeight/2);
                this.fade90.Color = new Color(255, 255, 255, (byte) Math.Max(0, this.fade90.Color.A - 15));
                MainRender.Draw(fade90, MainRender.OverlayLayer);

                Program.stage?.SetMusicVolume();
                Program.stage?.StopRoundTime();
                Program.stage?.ResetTimer();
                if (Program.stage.character_A.state.idle && Program.stage.character_B.state.idle && fade90.Color.A == 0) 
                    WGBattle.battle_state = State.RoundStart;
                break;

            case State.RoundStart: // Inicia a round
                if (Program.stage.CheckTimer(3)) {
                    Program.stage.ResetRoundTime();
                    Program.stage.StartRoundTime();
                    Program.stage.ReleasePlayers();
                    WGBattle.battle_state = State.Battling;
                }
                else if (Program.stage.CheckTimer(2))
                    UI.DrawText(S("fight!"), 0, -20, font: "style", color: new Color(240, 240, 255), layer: MainRender.ShadowLayer, TTS: true, TTS_id: "RoundFight", priority: true);
                else if (Program.stage.CheckTimer(1)) 
                    UI.DrawText(S("ready?"), 0, -30, font: "style_medium", color: new Color(240, 240, 255), layer: MainRender.ShadowLayer, TTS: true, TTS_id: "RoundReady", priority: true);
                else
                    UI.DrawText(S("round", " ", Program.stage.round.ToString()), 0, -30, font: "style_medium", color: new Color(240, 240, 255), layer: MainRender.ShadowLayer, TTS: true, TTS_id: "RoundStart", priority: true);
                break;

            case State.Battling: // Durante a batalha
                if (Program.stage.CheckRoundEnd()) {
                    WGBattle.battle_state = State.RoundEnd;
                    Program.stage.StopRoundTime();
                    Program.stage.ResetTimer();
                    Program.stage.PauseTimer();
                    Program.match_speed = Program.match_slow_speed;
                }
                break;

            case State.RoundEnd: // Fim de round
                if (Program.stage.CheckTimer(6)) {
                    Program.stage.LockPlayers();
                    Program.stage.ResetTimer();
                    if (Program.stage.CheckMatchEnd()) {
                        WGBattle.battle_state = State.MatchEnd;
                    } else {
                        WGBattle.battle_state = State.RoundStart;
                        Program.stage.ResetPlayers();
                    }

                } else if (Program.stage.CheckTimer(3)) {
                    if (WGBattle.round_winner == Drawn)
                        UI.DrawText(S("drawn"), 0, -30, font: "style_medium", color: new Color(240, 240, 255), layer: MainRender.ShadowLayer, TTS: true, TTS_id: "DrawResult", priority: true);
                    else
                        UI.DrawText(S(WGBattle.round_winner == 1 ? Program.stage.character_A.name : Program.stage.character_B.name, " ", "wins"), 0, -30, font: "style_medium", color: new Color(240, 240, 255), layer: MainRender.ShadowLayer, TTS: true, TTS_id: "RoundResult", priority: true);
                    Program.match_speed = Program.match_normal_speed;

                } else {
                    if (Program.stage.character_A.life_points.X <= 0 || Program.stage.character_B.life_points.X <= 0)
                        UI.DrawText(S("k.o."), 0, -20, font: "style", color: new Color(240, 240, 255), layer: MainRender.ShadowLayer, TTS: true, TTS_id: "KO_logo", priority: true);
                    else
                        UI.DrawText(S("time's up"), 0, -20, font: "style_medium", color: new Color(240, 240, 255), layer: MainRender.ShadowLayer, TTS: true, TTS_id: "TimesUp_logo", priority: true);

                    if (Program.stage.PlayersGrounded()) Program.stage.PauseTimer();
                } 
                break;

            case State.MatchEnd: // Fim da partida
                this.fade90.Position = new Vector2f(Camera.X - Config.RenderWidth/2, Camera.Y - Config.RenderHeight/2);
                this.fade90.Color = new Color(255, 255, 255, (byte) Math.Min(255, this.fade90.Color.A + 15));
                MainRender.Draw(fade90, MainRender.OverlayLayer);

                if (this.fade90.Color.A == 255) {
                    Camera.Reset();
                    Program.stage?.ResetMatch();
                    Program.stage?.ResetPlayers(force: true, total_reset: true);
                    WGBattle.battle_state = State.Intro;
                    Program.ChangeState(Program.State.PostBattle);
                }
                break;
        } 
    }
}