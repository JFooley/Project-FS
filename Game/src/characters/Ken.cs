using SFML.System;
using System.IO.Compression;
using SFML.Graphics;
using SFML.Audio;
using UI_space;
using System.Runtime.InteropServices;

public class Ken : Character {
    private static Dictionary<string, Texture> _shared_textures = new Dictionary<string, Texture>();
    public override Dictionary<string, Texture> textures {get => _shared_textures; protected set => _shared_textures = value ?? new Dictionary<string, Texture>();}
    
    private static Dictionary<string, SoundBuffer> _shared_sounds = new Dictionary<string, SoundBuffer>();
    public override Dictionary<string, SoundBuffer> sounds {get => _shared_sounds; protected set => _shared_sounds = value ?? new Dictionary<string, SoundBuffer>();}
    
    private static Dictionary<string, Frame[]> _shared_animations = new Dictionary<string, Frame[]>();
    public override Dictionary<string, Frame[]> animations { get => _shared_animations; protected set => _shared_animations = value ?? new Dictionary<string, Frame[]>();}

    private static Texture? _shared_palette;
    public override Texture palette {get => _shared_palette ?? new Texture(Data.textures["other:placeholder"]); protected set => _shared_palette = value;}

    private Fireball? current_fireball = null;
    private bool SA_flag = false;

    // Constructors
    public Ken(string initialState, int startX, int startY)
        : base("Ken", initialState, startX, startY, Data.GetPath("Assets/characters/Ken")) {
        this.life_points = new Vector2i(1000, 1000);
        this.dizzy_points = new Vector2i(500, 500);
        this.aura_points = new Vector2i(0, 100);
    } 
    public Ken() : base("Ken", "Idle", Data.GetPath("Assets/characters/Ken")) { }
    public override Character Copy() {
        var obj = new Ken("Intro", 0, 0);
        obj.Load();
        return obj;
    }
    public override void Load() {
        var fb = new Fireball();
        fb.LoadTextures();
        fb.LoadSounds();
        fb.LoadAnimations();
        fb.Load();

        var a = this.animations;

        this.states = new Dictionary<string, State> {
            { "Intro", new State(F(a["introFrames1"], a["introFrames2"], a["introFrames3"]), "Idle", can_be_hit: false)},
            { "Win", new State(F(a["win"]), "Idle", change_on_end: false, loop: false)},
            // Basic
            { "Idle", new State(F(a["idleFrames"]), "Idle", not_busy: true)},
            { "OnBlock", new State(F(a["OnBlockFrames"]), "Idle", on_block: true)}, 
            { "OnBlockLow", new State(F(a["OnBlockLowFrames"]), "Crouching", low: true, on_block: true)},
            { "OnHit", new State(F(a["OnHitFrames"]), "Idle", on_hit: true)},
            { "OnHitLow", new State(F(a["OnHitLowFrames"]), "Crouching", low: true, on_hit: true)},
            { "Airboned", new State(F(a["AirbonedFrames"]), "Falling", change_on_ground: true, change_on_end: false, loop: false, air: true, on_hit: true)},
            { "Parry", new State(F(a["parryFrames"]), "Idle", 6, glow: true, is_parry: true)},
            { "AirParry", new State(F(a["airParryFrames"]), "JumpFalling", 6, glow: true, air: true, is_parry: true)},
            // Normals
            { "LightP", new State(F(a["LPFrames"]), "Idle", 0, will_hit: true)},
            { "LowLightP", new State(F(a["lowLPFrames"]), "Crouching", 0, low: true, will_hit: true)},
            { "AirLightP", new State(F(a["airLPFrames"]), "Landing", 0, change_on_end: false, change_on_ground: true, loop: false, air: true, will_hit: true)},
            { "LightK", new State(F(a["LKFrames"]), "Idle", 0, will_hit: true)},
            { "LowLightK", new State(F(a["lowLKFrames"]), "Crouching", 1, hitstop: "Medium", low: true, will_hit: true)},
            { "AirLightK", new State(F(a["airLKFrames"]), "Landing", 0, change_on_end: false, change_on_ground: true, loop: false, air: true, will_hit: true)},
            { "MediumP", new State(F(a["MPFrames"]), "Idle", 1, hitstop: "Medium", will_hit: true)},
            { "LowMediumP", new State(F(a["lowMPFrames"]), "Crouching", 1, hitstop: "Medium", low: true, will_hit: true)},
            { "AirMediumP", new State(F(a["airMPFrames"]), "Landing", 1, hitstop: "Medium", change_on_end: false, change_on_ground: true, loop: false, air: true, will_hit: true)},
            { "BackMediumP", new State(F(a["backMPframes"]), "Idle", 2, hitstop: "Heavy", will_hit: true)},
            { "MediumK", new State(F(a["MKFrames"]), "Idle", 1, hitstop: "Medium", will_hit: true)},
            { "LowMediumK", new State(F(a["lowMKFrames"]), "Crouching", 2, hitstop: "Heavy", low: true, will_hit: true)},
            { "AirMediumK", new State(F(a["airMKFrames"]), "Landing", 1, hitstop: "Medium", change_on_end: false, change_on_ground: true, loop: false, air: true, will_hit: true)},
            { "BackMediumK", new State(F(a["BackMKFrames"]), "Idle", 1, will_hit: true)},
            { "CloseMP", new State(F(a["cl_HPFrames"]), "Idle", 1, hitstop: "Medium", will_hit: true)},
            // Throw
            { "Throw", new State(F(a["throw"]), "Idle", 5, will_hit: true, is_grab: true, hitstop: "None")},
            { "ThrowLeft", new State(F(a["throwLeft"]), "Idle", 5, will_hit: true, is_grab: true, hitstop: "None")},
            { "ThrowRight", new State(F(a["throwRight"]), "Idle", 5, will_hit: true, is_grab: true, hitstop: "None")},
            { "onThrow_KEN_RIGHT", new State(F(a["onThrow_KEN_RIGHT"]), "Falling", 0, has_gravity: false, on_hit: true)},
            { "onThrow_KEN_LEFT", new State(F(a["onThrow_KEN_LEFT"]), "Airboned", 0, has_gravity: false, on_hit: true)},
            // Movement
            { "WalkingForward", new State(F(a["walkingForwardFrames"]), "WalkingForward", not_busy: true)},
            { "WalkingBackward", new State(F(a["walkingBackwardFrames"]), "WalkingBackward", not_busy: true)},
            { "DashForward", new State(F(a["dashForwardFrames"]), "Idle", 20)},
            { "DashBackward", new State(F(a["dashBackwardFrames"]), "Idle", 20)},
            { "Jump", new State(F(a["jumpFrames"]), "JumpFalling", not_busy: true, air: true)},
            { "JumpForward", new State(F(a["jumpForward"]), "JumpFalling", not_busy: true, air: true)}, 
            { "JumpBackward", new State(F(a["JumpBackward"]), "JumpFalling", not_busy: true, air: true)},
            { "JumpFalling", new State(F(a["jumpFallingFrames"]), "Landing", not_busy: true, change_on_end: false, change_on_ground: true, loop: false, air: true)},
            { "Landing", new State(F(a["landingFrames"]), "Idle", not_busy: true)},
            { "CrouchingIn", new State(F(a["crouchingInFrames"]), "Crouching", not_busy: true, low: true)},
            { "Crouching", new State(F(a["crouchingFrames"]), "Crouching", not_busy: true, low: true)},
            // Super
            { "SA1", new State(F(a["SA1"]), "Idle", 4, trace: true, drama_wait: true, will_hit: true)},
            { "SA1_tail", new State(F(a["SA1_tail"]), "SA1_exit", 4, trace: true, air: true, drama_wait: true, will_hit: true)},
            { "SA1_exit", new State(F(a["SA1_exit"]), "JumpFalling", 4)},
            // Specials
            { "LightShory", new State(F(a["lightShoryFrames"]), "ShoryFalling", 3, hitstop: "Heavy", air: true, will_hit: true)},
            { "HeavyShory", new State(F(a["heavyShoryFrames"]), "ShoryFalling", 3, hitstop: "Heavy", air: true, will_hit: true)},
            { "ShoryEX", new State(F(a["EXShoryFrames"]), "ShoryFalling", 3, hitstop: "Heavy", glow: true, trace: true, air: true, can_be_hit: false, will_hit: true)},
            { "ShoryFalling", new State(F(a["shoryFallingFrames"]), "Landing", change_on_end: false, change_on_ground: true, loop: false, air: true, will_hit: true)},
            { "LightHaduken", new State(F(a["hadukenFrames"]), "Idle", 3, hitstop: "Medium", air: true, will_hit: true)},
            { "HeavyHaduken", new State(F(a["hadukenFrames"]), "Idle", 3, hitstop: "Medium", will_hit: true)},
            { "HadukenEX", new State(F(a["hadukenFrames"]), "Idle", 3, hitstop: "Heavy", glow: true, trace: true, will_hit: true)},
            { "LightTatso", new State(F(a["lightTatsoFrames"]), "Landing", 3, hitstop: "Light", will_hit: true)},
            { "HeavyTatso", new State(F(a["heavyTatsoFrames"]), "Landing", 3, hitstop: "Light", will_hit: true)},
            { "TatsoEX", new State(F(a["EXTatsoFrames"]), "Landing", 3, hitstop: "Light", glow: true, trace: true, will_hit: true)},
            { "AirTatso", new State(F(a["airTatsoFrames"]), "Landing", 3, change_on_ground: true, change_on_end: false, air: true, will_hit: true)},
            { "AirTatsoEX", new State(F(a["airTatsoFrames"]), "Landing", 3, change_on_ground: true, change_on_end: false, glow: true, trace: true, air: true, will_hit: true)},
            // Other
            { "Falling", new State(F(a["fallingFrames"]), "OnGround", can_be_hit: false)},
            { "Sweeped", new State(F(a["sweepedFrames"]), "Falling", on_hit: true, low: true, can_be_hit: false)},
            { "OnGround", new State(F(a["OnGroundFrames"]), "WakeUp", low: true, can_be_hit: false)},
            { "WakeUp", new State(F(a["wakeupFrames"]), "Idle", can_be_hit: false)},
        };
    }
    public override void Behave() {
        if (this.behave == false) return;

        // Crouching
        if (Input.Key_hold("Down", player: this.player_index, facing: this.facing) && !Input.Key_hold("Up", player: this.player_index, facing: this.facing) && this.not_acting) {
            this.ChangeState("CrouchingIn");
        }
        
        // Dashing
        if (Input.Key_sequence("Right Right", 10, flexEntry: false, flexTransition: false, player: this.player_index, facing: this.facing) && this.can_dash) {
            this.ChangeState("DashForward");
            return;
        } 
        else if (Input.Key_sequence("Left Left", 10, flexEntry: false, flexTransition: false, player: this.player_index, facing: this.facing) && this.can_dash) {
            this.ChangeState("DashBackward");
            return;
        }

        // Walking
        if (Input.Key_hold("Left", player: this.player_index, facing: this.facing) && !Input.Key_hold("Right", player: this.player_index, facing: this.facing) && this.not_acting && this.current_state != "WalkingBackward") {
            this.ChangeState("WalkingBackward");
        } else if (Input.Key_hold("Right", player: this.player_index, facing: this.facing) && !Input.Key_hold("Left", player: this.player_index, facing: this.facing) && this.not_acting && this.current_state != "WalkingForward") {
            this.ChangeState("WalkingForward");
        }

        // Fallback to Idle
        if ((this.current_state == "Crouching" && !Input.Key_hold("Down", player: this.player_index, facing: this.facing)) || (this.current_state == "WalkingForward" && !Input.Key_hold("Right", player: this.player_index, facing: this.facing)) || (this.current_state == "WalkingBackward" && !Input.Key_hold("Left", player: this.player_index, facing: this.facing))) {
            this.ChangeState("Idle");
        }

        // Jumps
        if (this.can_jump && Input.Key_hold("Up", player: this.player_index, facing: this.facing) && !Input.Key_hold("Left", player: this.player_index, facing: this.facing) && Input.Key_hold("Right", player: this.player_index, facing: this.facing)) {
            this.ChangeState("JumpForward");
            return;
        } else if (this.current_state == "JumpForward" && this.current_anim_frame_index == 1 && this.has_frame_change) {
            this.SetVelocity(
                X: 4, 
                Y: this.jump_hight);
        } 
        else if (this.can_jump && Input.Key_hold("Up", player: this.player_index, facing: this.facing) && Input.Key_hold("Left", player: this.player_index, facing: this.facing) && !Input.Key_hold("Right", player: this.player_index, facing: this.facing)) {
            this.ChangeState("JumpBackward");
            return;
        } else if (this.current_state == "JumpBackward" && this.current_anim_frame_index == 1 && this.has_frame_change) {
            this.SetVelocity(
                X: -4, 
                Y: this.jump_hight);
        }
        else if (this.can_jump && Input.Key_hold("Up", player: this.player_index, facing: this.facing)) {
            this.ChangeState("Jump");
            this.SetVelocity(X: 0, Y: this.jump_hight);
            return;
        }

        // Super
        if (Input.Key_sequence("Down Down RB", 10, player: this.player_index, facing: this.facing) && !this.on_air && (this.not_acting || this.not_acting_low || (this.has_hit && (this.current_state == "CloseMP" || this.current_state.Contains("Shory") || this.current_state == "LowLightK"))) && Character.CheckAuraPoints(this, 100)) {
            Character.UseAuraPoints(this, 100);
            this.ChangeState("SA1");
            this.stage.StopFor(16, 0, this);
            return;

        } else if (this.current_state == "SA1" && this.current_anim_frame_index == 3 && this.has_frame_change) {
            this.stage.PSSuper("SALighting", this.body.position.X, this.body.position.Y, X_offset: 50, Y_offset: -120, facing: this.facing);
            this.stage.StopFor(50);

        } else if (this.current_state == "SA1" && this.current_anim_frame_index >= 54 && this.has_hit && SA_flag) {
            this.ChangeState("SA1_tail");
            this.SA_flag = false;
            return;

        } else if (this.current_state == "SA1_tail" && this.current_anim_frame_index == 3 && this.has_frame_change ) {
            this.AddVelocity(X: 1, Y: 150);
            return;
        }

        // Shorys
        if (Input.Key_sequence("Right Down Right C", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low || this.has_hit && (this.current_state == "MediumP" || this.current_state == "LightP" || this.current_state == "CloseMP" || this.current_state == "LowLightK" || this.current_state == "LowMediumP"))) {
            this.ChangeState("LightShory");
            return;
        } else if (this.current_state == "LightShory" && this.current_anim_frame_index == 3 && this.has_frame_change) {
            this.AddVelocity(
                X: this.has_hit ? 0f : 1.5f,
                Y: 50);
        } 
        if (Input.Key_sequence("Right Down Right D", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low || this.has_hit && (this.current_state == "LowMediumP" || this.current_state == "LowLightK" ))) {
            this.ChangeState("HeavyShory");
            return;
        } else if (this.current_state == "HeavyShory" && this.current_anim_frame_index == 3 && this.has_frame_change) {
            this.AddVelocity(
                X: this.has_hit ? 0f : 3f, 
                Y: 80);
        } 
        if (Input.Key_sequence("Right Down Right RB", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low || this.has_hit && (this.current_state == "LowMediumP" || this.current_state == "LowLightK")) && Character.CheckAuraPoints(this, 50)) {
            Character.UseAuraPoints(this, 50);
            this.ChangeState("ShoryEX");
            return;
        } else if (this.current_state == "ShoryEX" && this.current_anim_frame_index == 3 && this.has_frame_change) {
            this.AddVelocity(
                X: this.has_hit ? 0f : 5f, 
                Y: 100);
        } 
        if (this.current_state.Contains("Shory") && this.state.animation.on_last_frame && Math.Sign(this.body.velocity.X) == this.facing) this.body.velocity.X = 0;

        // Haduken
        if (this.current_fireball != null && this.current_fireball.remove) this.current_fireball = null;
        if (this.current_fireball == null && Input.Key_sequence("Down Right C", 10, player: this.player_index, facing: this.facing) && ((this.not_acting || this.not_acting_low) || (this.has_hit && (this.current_state == "MediumP" || this.current_state == "LightP" || this.current_state == "LowLightK")))) {
            this.ChangeState("LightHaduken");
            return;
        } else if (this.current_state == "LightHaduken" && this.current_anim_frame_index == 3 && this.has_frame_change) {
            this.spawnFireball("Ken1", this.body.position.X, this.body.position.Y - 5, this.facing, this.player_index, X_offset: 25);
        } 
        if (this.current_fireball == null && Input.Key_sequence("Down Right D", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low)) {
            this.ChangeState("HeavyHaduken");
            return;
        } else if (this.current_state == "HeavyHaduken" && this.current_anim_frame_index == 4 && this.has_frame_change) {
            this.spawnFireball("Ken2", this.body.position.X, this.body.position.Y - 5, this.facing, this.player_index, X_offset: 25);
        }
        if (this.current_fireball == null && Input.Key_sequence("Down Right RB", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low) && Character.CheckAuraPoints(this, 50)) {
            Character.UseAuraPoints(this, 50);
            this.ChangeState("HadukenEX");
            return;
        } else if (this.current_state == "HadukenEX" && this.current_anim_frame_index == 4 && this.has_frame_change) {
            this.spawnFireball("Ken3", this.body.position.X, this.body.position.Y - 5, this.facing, this.player_index, life_points: 2, X_offset: 25);
            this.PlaySound("EX");
        }

        // Tatso
        if (Input.Key_sequence("Down Left A", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low || (this.has_hit && (this.current_state == "LowLightK" || this.current_state == "CloseMP")))) {
            this.ChangeState("LightTatso");
            this.SetVelocity(Y: 5);
            return;
        } else if (this.current_state == "LightTatso") {
            this.AddVelocity(Y: 0.55f, raw_set: true);

        } else if (Input.Key_sequence("Down Left B", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low || (this.has_hit && (this.current_state == "LowLightK" || this.current_state == "CloseMP")))) {
            this.ChangeState("HeavyTatso");
            this.SetVelocity(Y: 5);
            return;
        } else if (this.current_state == "HeavyTatso") {
            this.AddVelocity(Y: 0.59f, raw_set: true);

        } else if (Input.Key_sequence("Down Left RB", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low || (this.has_hit && (this.current_state == "LowLightK" || this.current_state == "CloseMP"))) && Character.CheckAuraPoints(this, 50)) {
            Character.UseAuraPoints(this, 50);
            this.ChangeState("TatsoEX");
            this.SetVelocity(Y: 5);
            return;
        } else if (this.current_state == "TatsoEX") {
            this.AddVelocity(Y: 0.55f, raw_set: true);
        }
        
        // Air tatso
        if ((Input.Key_sequence("Down Left B", 10, player: this.player_index, facing: this.facing) || Input.Key_sequence("Down Left A", 10, player: this.player_index, facing: this.facing)) && this.not_acting_air) {
            this.ChangeState("AirTatso");
            this.PlaySound("tatso");
            return;
        } else if (Input.Key_sequence("Down Left RB", 10, player: this.player_index, facing: this.facing) && this.not_acting_air && Character.CheckAuraPoints(this, 50)) {
            Character.UseAuraPoints(this, 50);
            this.ChangeState("AirTatsoEX");
            this.PlaySound("EX");
            this.PlaySound("tatso");
            return;
        } 

        // Throw
        if ((Input.Key_press("LB", this.player_index, this.facing) || Input.Key_press("A C", this.player_index, this.facing)) && this.not_acting) {
            this.ChangeState("Throw");
            return;
        }

        // Fast wakeup
        if (Input.Key_hold("Up", player: this.player_index, facing: this.facing) && this.current_state == "OnGround") {
            this.ChangeState(this.state.post_state);
        }

        // Normals plus
        if (Input.Key_press("D", player: this.player_index, facing: this.facing, input_window: Config.hit_stop_time) && this.has_hit && this.current_state == "LightP") {
            this.SetVelocity();
            this.ChangeState("CloseMP");
            return;
        } else if (Input.Key_press("B", player: this.player_index, facing: this.facing) && Input.Key_hold("Left", player: this.player_index, facing: this.facing) && this.not_acting && !this.crounching) {
            this.ChangeState("BackMediumK");
            return;
        } else if (Input.Key_press("D", player: this.player_index, facing: this.facing) && Input.Key_hold("Left", player: this.player_index, facing: this.facing) && (this.not_acting || (this.has_hit && this.current_state == "CloseMP"))) {
            this.ChangeState("BackMediumP");
            return;
        } 

        // Normals
        if (this.not_acting_low) {
            if (Input.Key_press("C", player: this.player_index, facing: this.facing)) this.ChangeState("LowLightP");
            else if (Input.Key_press("A", player: this.player_index, facing: this.facing)) this.ChangeState("LowLightK");
            else if (Input.Key_press("D", player: this.player_index, facing: this.facing)) this.ChangeState("LowMediumP");
            else if (Input.Key_press("B", player: this.player_index, facing: this.facing)) this.ChangeState("LowMediumK");
        } else if (this.not_acting) {
            if (Input.Key_press("C", player: this.player_index, facing: this.facing)) this.ChangeState("LightP");     
            else if (Input.Key_press("A", player: this.player_index, facing: this.facing)) this.ChangeState("LightK");     
            else if (Input.Key_press("D", player: this.player_index, facing: this.facing)) this.ChangeState("MediumP");     
            else if (Input.Key_press("B", player: this.player_index, facing: this.facing) ) this.ChangeState("MediumK");
        } else if (this.not_acting_air) {
            if (Input.Key_press("C", player: this.player_index, facing: this.facing)) this.ChangeState("AirLightP");
            else if (Input.Key_press("A", player: this.player_index, facing: this.facing)) this.ChangeState("AirLightK");
            else if (Input.Key_press("D", player: this.player_index, facing: this.facing)) this.ChangeState("AirMediumP");
            else if (Input.Key_press("B", player: this.player_index, facing: this.facing)) this.ChangeState("AirMediumK");
        }
    }
    public override int ImposeBehavior(Character target) {
        int hit = -1;

        switch (this.current_state) {
            case "LightP":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 3);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 13, 50);
                    target.Stun(this, 6);
                }
                Character.Push(target: target, self: this, Config.light_pushback);
                Character.AddAuraPoints(target, this, hit);
                break;
                
            case "LowLightP":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 3);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 13, 50);
                    target.Stun(this, 6);
                }
                Character.Push(target: target, self: this, Config.light_pushback);
                Character.AddAuraPoints(target, this, hit);
                break;

            case "AirLightP":
                if (target.isBlockingHigh()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 7);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 50, 100);
                    target.Stun(this, 7);
                }
                Character.Push(target: target, self: this, Config.light_pushback, force_push: true);
                Character.AddAuraPoints(target, this, hit);
                break;

            case "LightK":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 2);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 44, 50);
                    target.Stun(this, 2);
                }
                Character.Push(target: target, self: this, Config.light_pushback);
                Character.AddAuraPoints(target, this, hit);
                break;
                
            case "LowLightK":
                if (target.isBlockingLow()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -4);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 85, 50);
                    target.Stun(this, -3);
                }
                Character.Push(target: target, self: this, Config.light_pushback);
                Character.AddAuraPoints(target, this, hit);
                break;
            
            case "AirLightK":
                if (target.isBlockingHigh()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 10);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 90, 150);
                    target.Stun(this, 10);
                }
                Character.Push(target: target, self: this, Config.light_pushback, force_push: true);
                Character.AddAuraPoints(target, this, hit);
                break;

            case "MediumP":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -2);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 100, 172);
                    target.Stun(this, 0);
                }
                Character.Push(target: target, self: this, Config.medium_pushback);
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "LowMediumP":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 3);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 90, 100);
                    target.Stun(this, 4);
                }
                Character.Push(target: target, self: this, Config.medium_pushback);
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "AirMediumP":
                if (target.isBlockingHigh()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 10);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 100, 180);
                    target.Stun(this, 10);
                }
                Character.Push(target: target, self: this, Config.medium_pushback, force_push: true);
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "MediumK":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -6);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 120, 235);
                    target.Stun(this, 0);
                }
                Character.Push(target: target, self: this, Config.medium_pushback);
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "LowMediumK":
                if (target.isBlockingLow()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -14);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 110, 200);
                    target.Stun(this, 0, sweep: true);
                }
                Character.Push(target: target, self: this, Config.medium_pushback);
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "AirMediumK":
                if (target.isBlockingHigh()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 13);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 125, 235);
                    target.Stun(this, 13);
                }
                Character.Push(target: target, self: this, Config.medium_pushback, force_push: true);
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "BackMediumK":
                if (target.isBlockingHigh()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 1);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 56, 94);
                    target.Stun(this, 10);
                }
                Character.Push(target: target, self: this, Config.heavy_pushback);
                Character.AddAuraPoints(target, this, hit, self_amount: 6);
                break;

            case "BackMediumP":
                if (target.isBlockingHigh()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 1);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 56, 94);
                    target.Stun(this, 10);
                }
                Character.Push(target: target, self: this, Config.heavy_pushback);
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "CloseMP":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -2);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 80, 100);
                    target.Stun(this, 0);
                }
                Character.Push(target: target, self: this, Config.medium_pushback);
                Character.AddAuraPoints(target, this, hit);
                break;
            
            case "Throw":
                if (target.on_air) break;

                target.SetVelocity(raw_set: true);
                target.SetAcceleration();
                target.body.position = new Vector2f(this.body.position.X + (this.facing * 20), this.body.position.Y);

                if (Input.Key_hold("Left", this.player_index, this.facing)) {
                    this.ChangeState("ThrowLeft");
                    target.ChangeState("onThrow_KEN_LEFT");
                } else {
                    this.ChangeState("ThrowRight");
                    target.ChangeState("onThrow_KEN_RIGHT");
                }
                
                hit = Character.GRAB;
                break;
            
            case "ThrowLeft":
                target.SetVelocity(-5, 20);
                Character.Damage(target: target, self: this, 100, 203);
                Character.AddAuraPoints(target, this, hit, self_amount: 10);
                break;

            case "ThrowRight":
                Character.Damage(target: target, self: this, 130, 203);
                Character.AddAuraPoints(target, this, hit, self_amount: 10);
                break;

            case "LightShory":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -10);
                    Character.Push(target: target, self: this, 1.1f);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 80, 160);
                    target.Stun(this, 0, airbone: true);
                    Character.Push(target: target, self: this, 1.1f, Y_amount: 110f, airbone: true, fixed_height: true);
                }
                Character.AddAuraPoints(target, this, hit);
                break;

            case "HeavyShory":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -17);
                    Character.Push(target: target, self: this, 2.0f);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 100, 85);
                    if (this.current_anim_frame_index >= 3) {
                        target.Stun(this, 0, airbone: true);
                        Character.Push(target: target, self: this, 2.0f, Y_amount: 100f, airbone: true, fixed_height: true);
                    } else {
                        target.Stun(this, 0);
                    }
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "ShoryEX":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    Character.Damage(target: target, self: this, 20, 20);
                    target.BlockStun(this, -10);
                    Character.Push(target: target, self: this, 2.5f);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 120, 85);
                    if (this.current_anim_frame_index >= 3) {
                        target.Stun(this, 0, airbone: true);
                        Character.Push(target: target, self: this, 2.5f, Y_amount: 110f, airbone: true, fixed_height: true);
                    } else {
                        target.Stun(this, 0);
                    }

                }
                Character.AddAuraPoints(target, this, hit, self_amount: 2);
                break;
            
            case "LightTatso":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    Character.Damage(target: target, self: this, 10, 20);
                    target.BlockStun(this, 12, raw_value: true);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 66, 203);
                    target.Stun(this, 15, raw_value: true);
                }
                Character.Push(target: target, self: this, Config.light_pushback);
                Character.AddAuraPoints(target, this, hit);
                break;
            
            case "HeavyTatso":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    Character.Damage(target: target, self: this, 10, 30);
                    target.BlockStun(this, 12, raw_value: true);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 54, 234);
                    target.Stun(this, 15, raw_value: true);
                }
                Character.Push(target: target, self: this, Config.light_pushback);
                Character.AddAuraPoints(target, this, hit, self_amount: 8);
                break;

            case "TatsoEX":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    Character.Damage(target: target, self: this, 10, 30);
                    target.BlockStun(this, 6, raw_value: true);
                    
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 54, 234);
                    target.Stun(this, 8, raw_value: true);
                }
                Character.Push(target: target, self: this, Config.light_pushback);
                Character.AddAuraPoints(target, this, hit, self_amount: 2);
                break;
            
            case "AirTatso":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    Character.Damage(target: target, self: this, 10, 30);
                    target.BlockStun(this, 15, raw_value: true);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 80, 140);
                    target.Stun(this, 15, raw_value: true);
                }
                Character.Push(target: target, self: this, Config.light_pushback);
                Character.AddAuraPoints(target, this, hit, self_amount: 14);
                break;

            case "AirTatsoEX":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    Character.Damage(target: target, self: this, 10, 20);
                    target.BlockStun(this, 1);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 80, 140);
                    target.Stun(this, -6, airbone: true);
                    Character.Push(target: target, self: this, 2f, Y_amount: 50, airbone: true);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 4);
                break;

            case "SA1":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    Character.Damage(target: target, self: this, 5, 0);
                    target.BlockStun(this, 30, raw_value: true);
                    Character.Push(target: target, self: this, Config.heavy_pushback);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 50, 35);
                    target.Stun(this, 10, force_stand: true);
                    Character.Push(target: target, self: this, 1, Y_amount: 20);
                    SA_flag = true;
                }
                break;
                
            case "SA1_tail":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    Character.Damage(target: target, self: this, 5, 0);
                    target.BlockStun(this, 30, raw_value: true);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 45, 35);
                    target.Stun(this, 5, airbone: true);

                    if (this.current_anim_frame_index < 6) {
                        Character.Push(target: target, self: this, 1, Y_amount: 150, airbone: true);
                    } else if (this.current_anim_frame_index >= 13) {
                        Character.Push(target: target, self: this, Config.heavy_pushback, Y_amount: 80, airbone: true);
                    }
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 2);
                break;
                
            case "Shungoku":
                Character.Damage(target: target, self: this, 400, 0);

                this.stage.PSSuper("Shungoku", target.body.position.X, this.body.position.Y, Y_offset: -125, facing: this.facing);
                this.stage.PSSuper("Shungoku_text", Camera.X, Camera.Y);
                this.stage.StopFor(40 * 4); // 40 frames a 15 fps

                this.ChangeState("Shungoku_End");
                this.SetVelocity();
                this.body.position.X = target.body.position.X - 100 * this.facing;

                target.SetVelocity();
                target.ChangeState("OnGround");
                break;

            default:
                break;
        }

        return hit;
    }
    
    // AI
    public override void SelectMovement(FightState f_state) {
        if (f_state.enemyDistance < 0.3f) {
            var choise = AI.rand.Next(0, 20);
            if (choise == 0 && f_state.onCorner) { 
                this.BOT.EnqueueMove("Up Right", 5);
            } else if (choise <= 2) { 
                this.BOT.EnqueueMove("Left", 5);
            } else if (choise <= 5) {
                this.BOT.EnqueueMove("Right", 20);
            } else if (choise <= 8) {
                this.BOT.EnqueueMove("Down", 30);
            } 
        } else if (f_state.enemyDistance < 0.7f) {
            var choise = AI.rand.Next(0, 20);
            if (choise == 0 || choise == 20) {
                this.BOT.EnqueueMove("Up Right", 5);

            } else if (choise <= 2) { 
                this.BOT.EnqueueMove("Right", 20);

            } else if (choise <= 4) { 
                this.BOT.EnqueueMove("Right", 20);
                this.BOT.EnqueueMove("Down", 10);

            } else if (choise <= 6) { 
                this.BOT.EnqueueMove("Left", 10);

            } else if (choise <= 8) { 
                this.BOT.EnqueueMove("Left", 10);
                this.BOT.EnqueueMove("Down", 10);
                this.BOT.EnqueueMove("Right", 20);  

            } else if (choise <= 10) {
                this.BOT.EnqueueMove("Left", 10);
                this.BOT.EnqueueMove("Right", 30);

            } else if (choise == 11) {
                this.BOT.EnqueueMove("Right *", 2);
                this.BOT.EnqueueMove("*", 2);
                this.BOT.EnqueueMove("Right *", 2);
            }
        } else {
            var choise = AI.rand.Next(0, 10);
            if (choise == 1) { 
                this.BOT.EnqueueMove("Down", 30);
            } else if (choise < 6) {
                this.BOT.EnqueueMove("Right", 20);
            }
        } 
        this.BOT.EnqueueMove("", AI.rand.Next(0, 20 * this.BOT.difficulty));
    }
    public override void SelectAction(FightState f_state) {
        if (f_state.enemyIsDead) return;
        if (this.state.will_hit && !this.on_hit && !this.state.on_block && !this.state.on_parry) return;

        if (this.on_hit || this.state.on_block || ((f_state.enemyIsAttacking || f_state.enemyIsGrabbing) && f_state.enemyDistance <= 0.5f)) { // Block
            if (f_state.enemyIsGrabbing) this.BOT.EnqueueAction("LB *", 5);
            else if (f_state.enemyIsCrouching) this.BOT.EnqueueAction("Left Down *", 5);
            else this.BOT.EnqueueAction("Left *", 5);
            return;
        }
        
        if (f_state.enemyIsAirborne && f_state.enemyDistance <= 0.3f) { // Anti air
            var choise = AI.rand.Next(0, this.BOT.difficulty);
            if (choise == 0) { // Shoryuken
                this.BOT.EnqueueAction("Right *", 2);
                this.BOT.EnqueueAction("Down *", 2);
                this.BOT.EnqueueAction("Right *", 2);
                this.BOT.EnqueueAction("C", 5);
            } else if (choise == 1) {
                this.BOT.EnqueueAction("Down D *", 5);
            }

        } else if (this.state.air && f_state.enemyDistance < 0.5f) { // Air
            var choise = AI.rand.Next(0, 4);

            if (choise == 0) {
                this.BOT.EnqueueAction("A", 3);
            } else if (choise == 1) {
                this.BOT.EnqueueAction("B", 3);
            } else if (choise == 2) {
                this.BOT.EnqueueAction("D", 3);
            } else {
                this.BOT.EnqueueAction("Down *", 5);
                this.BOT.EnqueueAction("Left *", 5);

                if (Character.CheckAuraPoints(this, 50))
                    this.BOT.EnqueueAction("RB", 1);
                else
                    this.BOT.EnqueueAction("A", 1);
            }
            
        } else if (f_state.enemyDistance < 0.3f) { // Close range
            var choise = AI.rand.Next(0, 10);
            if (f_state.enemyIsCrouching) { // LOW
                if (choise < 3) { // Normais
                    choise = AI.rand.Next(0, 2);
                    if (choise < 1) this.BOT.EnqueueAction("A", 3);
                    else if (choise < 2) this.BOT.EnqueueAction("B", 3);

                } else if (choise < 5) { // Low LK
                    this.BOT.EnqueueAction("Down *", 3);
                    this.BOT.EnqueueAction("Down A *", 3);
                    this.BOT.EnqueueAction("", 3);

                    choise = AI.rand.Next(0, 10);
                    if (choise < 1) { // Path: Light Tatso
                        this.BOT.EnqueueAction("Down *", 3);
                        this.BOT.EnqueueAction("Left *", 3);
                        this.BOT.EnqueueAction("A *", 3);

                    } else if (choise < 3 && Character.CheckAuraPoints(this, 50)) { // Path: Tatso
                        this.BOT.EnqueueAction("Down *", 3);
                        this.BOT.EnqueueAction("Left *", 3);

                        if (choise == 1) this.BOT.EnqueueAction("RB *", 3);
                        else this.BOT.EnqueueAction("A *", 3);

                    } else if (choise < 5) { // Path: Light Shoryuken
                        this.BOT.EnqueueAction("Right *", 3);
                        this.BOT.EnqueueAction("Down *", 3);
                        this.BOT.EnqueueAction("Right *", 3);
                        this.BOT.EnqueueAction("C *", 3);

                    }
                } else if (choise < 6) { // Sweep
                    this.BOT.EnqueueAction("Down *", 5);
                    this.BOT.EnqueueAction("Down B *", 5);
                } else if (choise < 7) { // Overhead
                    this.BOT.EnqueueAction("Left B *", 5);
                }

            } else { // STAND
                if (choise < 2) { // Normais
                    choise = AI.rand.Next(0, 4);
                    if (choise < 1) this.BOT.EnqueueAction("D", 3);
                    else if (choise < 2) this.BOT.EnqueueAction("B", 3);
                    else if (choise < 3) this.BOT.EnqueueAction("C", 3);
                    else this.BOT.EnqueueAction("A", 3);

                } else if (choise == 3) { // Grab
                    choise = AI.rand.Next(0, 2);
                    if (choise == 0) this.BOT.EnqueueAction("LB *", 5);
                    else this.BOT.EnqueueAction("LB Left *", 5);

                } else if (choise == 4) { // Low LK
                    this.BOT.EnqueueAction("Down *", 3);
                    this.BOT.EnqueueAction("Down A *", 3);
                    this.BOT.EnqueueAction("", 3);

                    choise = AI.rand.Next(0, 10);
                    if (choise == 0) { // Path: Light Tatso
                        this.BOT.EnqueueAction("Down *", 3);
                        this.BOT.EnqueueAction("Left *", 3);
                        this.BOT.EnqueueAction("A *", 3);

                    } else if (choise == 1 && Character.CheckAuraPoints(this, 50)) { // Path: EX Tatso
                        this.BOT.EnqueueAction("Down *", 3);
                        this.BOT.EnqueueAction("Left *", 3);
                        this.BOT.EnqueueAction("RB *", 3);

                    } else if (choise == 3) { // Path: Light Shoryuken
                        this.BOT.EnqueueAction("Right *", 3);
                        this.BOT.EnqueueAction("Down *", 3);
                        this.BOT.EnqueueAction("Right *", 3);
                        this.BOT.EnqueueAction("C *", 3);

                    } else if (Character.CheckAuraPoints(this, 100)) { // Path: Super Art 1
                        this.BOT.EnqueueAction("Down *", 3);
                        this.BOT.EnqueueAction(" *", 3);
                        this.BOT.EnqueueAction("Down *", 3);
                        this.BOT.EnqueueAction("RB *", 3);

                    }

                } else if (choise == 5) { // Sweep
                    this.BOT.EnqueueAction("Down *", 5);
                    this.BOT.EnqueueAction("Down B *", 5);

                } else if (choise == 6) { // Light Shory
                    this.BOT.EnqueueAction("Right *", 5);
                    this.BOT.EnqueueAction("Down *", 5);
                    this.BOT.EnqueueAction("Right *", 5);
                    this.BOT.EnqueueAction("C", 1);

                } else if (choise == 7) { // Tatso                
                    this.BOT.EnqueueAction("Down *", 5);
                    this.BOT.EnqueueAction("Left *", 5);

                    choise = AI.rand.Next(0, 10);
                    if (choise == 0 && Character.CheckAuraPoints(this, 50)) this.BOT.EnqueueAction("RB", 1);
                    else if (choise < 4) this.BOT.EnqueueAction("B", 1);
                    else this.BOT.EnqueueAction("A", 1);

                } else if (choise < 10) { // Target combo
                    this.BOT.EnqueueAction("C", 3);
                    this.BOT.EnqueueAction("D", 3);
                    this.BOT.EnqueueAction("*", 3);

                    choise = AI.rand.Next(0, 4);
                    if (choise == 0) { // Path: BackMP
                        this.BOT.EnqueueAction("Left *", 3);
                        this.BOT.EnqueueAction("Left D *", 5);

                    } else if (choise == 1) { // Path: Light Shoryuken
                        this.BOT.EnqueueAction("Right *", 5);
                        this.BOT.EnqueueAction("Down *", 5);
                        this.BOT.EnqueueAction("Right *", 5);
                        this.BOT.EnqueueAction("C", 1);

                    } else if (Character.CheckAuraPoints(this, 100)) { // Path: Super Art 1
                        this.BOT.EnqueueAction("Down *", 3);
                        this.BOT.EnqueueAction("*", 3);
                        this.BOT.EnqueueAction("Down *", 3);
                        this.BOT.EnqueueAction("RB", 3);
                    }
                }
            }

        } else if (f_state.enemyDistance < 0.4f) { // Mid range
            var choise = AI.rand.Next(0, 10);
            if (choise < 1) { // Medium kick
                this.BOT.EnqueueAction("B", 1);

            } else if (choise < 2) { // Low LK
                this.BOT.EnqueueAction("Right *", 20);
                this.BOT.EnqueueAction("Down A *", 2);

                if (AI.rand.Next(0, 10) < 3) { // Follow up with Tatso
                    choise = AI.rand.Next(0, 10);
                    if (choise == 0 && Character.CheckAuraPoints(this, 50)) {  // EX Tatso
                        this.BOT.EnqueueAction("Down *", 5);
                        this.BOT.EnqueueAction("Left *", 5);
                        this.BOT.EnqueueAction("RB", 1);

                    } else if (choise < 4) { // Heavy Tatso
                        this.BOT.EnqueueAction("Down *", 5);
                        this.BOT.EnqueueAction("Left *", 5);
                        this.BOT.EnqueueAction("B", 1);

                    } else { // Light Tatso
                        this.BOT.EnqueueAction("Down *", 5);
                        this.BOT.EnqueueAction("Left *", 5);
                        this.BOT.EnqueueAction("A", 1);
                    }
                }
            } else if (choise < 3) { // Tatso
                choise = AI.rand.Next(0, 10);
                this.BOT.EnqueueAction("Down *", 15);
                this.BOT.EnqueueAction("Left *", 15);
                
                if (choise == 0 && Character.CheckAuraPoints(this, 50)) {  // EX Tatso
                    this.BOT.EnqueueAction("RB", 1);
                } else if (choise < 4) { // Heavy Tatso
                    this.BOT.EnqueueAction("B", 1);
                } else { // Light Tatso
                    this.BOT.EnqueueAction("A", 1);
                }
            } 

        } else { // Long range
            if (AI.rand.Next(0, 5) == 0 && this.current_fireball == null) {  // Hadouken
                this.BOT.EnqueueAction("Down *", 20);
                this.BOT.EnqueueAction("Right *", 5);

                var choise = AI.rand.Next(0, 3);
                if (choise == 0 && Character.CheckAuraPoints(this, 50)) 
                    this.BOT.EnqueueAction("RB", 5);
                else if (choise == 1) 
                    this.BOT.EnqueueAction("D", 5);
                else 
                    this.BOT.EnqueueAction("C", 5);

                return;
            } else 
                this.BOT.EnqueueAction("", AI.rand.Next(10, 30)); // Nothing
        }
    
        this.BOT.EnqueueAction("", AI.rand.Next(0, 20 * this.BOT.difficulty));
    }

    // Other
    public override void Reset(int start_point, int facing, String state = "Idle", bool total_reset = false) {
        base.Reset(start_point, facing, state, total_reset);
        this.current_fireball = null;
    }
    public void spawnFireball(string state, float X, float Y, int facing, int team, int life_points = 1, int X_offset = 10, int Y_offset = 0) {        
        var fb = new Fireball(state, life_points, X + X_offset * facing, Y + Y_offset, team, facing);
        fb.ChangeState(state, reset: true);
        fb.Load();
        Program.stage?.newCharacters.Add(fb);
        this.current_fireball = fb;
    }
    public void spawnShungoku(string state, float X, float Y, int facing) {
        var fb = new Shungokusatso(state, X, Y, facing);
        fb.ChangeState(state, reset: true);
        fb.Load();
        Program.stage?.newCharacters.Add(fb);
    }
}