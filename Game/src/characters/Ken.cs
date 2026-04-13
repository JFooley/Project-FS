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
    
    private static Dictionary<string, List<Frame>> _shared_animations = new Dictionary<string, List<Frame>>();
    public override Dictionary<string, List<Frame>> animations { get => _shared_animations; protected set => _shared_animations = value ?? new Dictionary<string, List<Frame>>();}

    private static Texture? _shared_palette;
    public override Texture palette {get => _shared_palette ?? new Texture(Data.textures["other:placeholder"]); protected set => _shared_palette = value;}

    private Fireball? current_fireball = null;

    // Constructors
    public Ken(string initialState, int startX, int startY)
        : base("Ken", initialState, startX, startY, Data.GetPath("Assets/characters/Ken"))
    {
        this.life_points = new Vector2i(1000, 1000);
        this.dizzy_points = new Vector2i(500, 500);
        this.aura_points = new Vector2i(0, 100);
    } 
    public Ken() : base("Ken", Data.GetPath("Assets/characters/Ken")) { }
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
            // Basic
            { "Idle", new State(a["idleFrames"], "Idle", not_busy: true)},
            { "OnBlock", new State(a["OnBlockFrames"], "Idle", on_block: true)}, 
            { "OnBlockLow", new State(a["OnBlockLowFrames"], "Crouching", low: true, on_block: true)},
            { "OnHit", new State(a["OnHitFrames"], "Idle", on_hit: true)},
            { "OnHitLow", new State(a["OnHitLowFrames"], "Crouching", low: true, on_hit: true)},
            { "Airboned", new State(a["AirbonedFrames"], "Falling", change_on_ground: true, change_on_end: false, loop: false, air: true, on_hit: true)},
            { "Parry", new State(a["parryFrames"], "Idle", 6, glow: true, is_parry: true)},
            { "AirParry", new State(a["airParryFrames"], "JumpFalling", 6, glow: true, air: true, is_parry: true)},
            // Normals
            { "LightP", new State(a["LPFrames"], "Idle", 0, can_harm: true)},
            { "LowLightP", new State(a["lowLPFrames"], "Crouching", 0, low: true, can_harm: true)},
            { "AirLightP", new State(a["airLPFrames"], "Idle", 0, change_on_end: false, change_on_ground: true, loop: false, air: true, can_harm: true)},
            { "LightK", new State(a["LKFrames"], "Idle", 0, can_harm: true)},
            { "LowLightK", new State(a["lowLKFrames"], "Crouching", 1, hitstop: "Medium", low: true, can_harm: true)},
            { "AirLightK", new State(a["airLKFrames"], "Idle", 0, change_on_end: false, change_on_ground: true, loop: false, air: true, can_harm: true)},
            { "MediumP", new State(a["MPFrames"], "Idle", 1, hitstop: "Medium", can_harm: true)},
            { "LowMediumP", new State(a["lowMPFrames"], "Crouching", 1, hitstop: "Medium", low: true, can_harm: true)},
            { "AirMediumP", new State(a["airMPFrames"], "Idle", 1, hitstop: "Medium", change_on_end: false, change_on_ground: true, loop: false, air: true, can_harm: true)},
            { "BackMediumP", new State(a["backMPframes"], "Idle", 2, hitstop: "Heavy", can_harm: true)},
            { "MediumK", new State(a["MKFrames"], "Idle", 1, hitstop: "Medium", can_harm: true)},
            { "LowMediumK", new State(a["lowMKFrames"], "Crouching", 2, hitstop: "Heavy", low: true, can_harm: true)},
            { "AirMediumK", new State(a["airMKFrames"], "Idle", 1, hitstop: "Medium", change_on_end: false, change_on_ground: true, loop: false, air: true, can_harm: true)},
            { "BackMediumK", new State(a["BackMKFrames"], "Idle", 1, can_harm: true)},
            { "CloseMP", new State(a["cl_HPFrames"], "Idle", 1, hitstop: "Medium", can_harm: true)},
            // Movement
            { "WalkingForward", new State(a["walkingForwardFrames"], "WalkingForward", not_busy: true)},
            { "WalkingBackward", new State(a["walkingBackwardFrames"], "WalkingBackward", not_busy: true)},
            { "DashForward", new State(a["dashForwardFrames"], "Idle", 20)},
            { "DashBackward", new State(a["dashBackwardFrames"], "Idle", 20)},
            { "Jump", new State(a["jumpFrames"], "JumpFalling", not_busy: true, air: true)},
            { "JumpForward", new State(a["jumpForward"], "JumpFalling", not_busy: true, air: true)}, 
            { "JumpBackward", new State(a["JumpBackward"], "JumpFalling", not_busy: true, air: true)},
            { "JumpFalling", new State(a["jumpFallingFrames"], "Landing", not_busy: true, change_on_end: false, change_on_ground: true, loop: false, air: true)},
            { "Landing", new State(a["landingFrames"], "Idle", not_busy: true)},
            { "CrouchingIn", new State(a["crouchingInFrames"], "Crouching", not_busy: true, low: true)},
            { "Crouching", new State(a["crouchingFrames"], "Crouching", not_busy: true, low: true)},
            // Super
            { "SA1", new State(a["SA1"], "MediumK", 4, trace: true, drama_wait: true, can_harm: true)},
            { "SA1_tail", new State(a["SA1_tail"], "SA1_exit", 4, trace: true, air: true, drama_wait: true, can_harm: true)},
            { "SA1_exit", new State(a["SA1_exit"], "JumpFalling", 4)},
            // Specials
            { "LightShory", new State(a["lightShoryFrames"], "ShoryFalling", 3, hitstop: "Heavy", air: true, can_harm: true)},
            { "HeavyShory", new State(a["heavyShoryFrames"], "ShoryFalling", 3, hitstop: "Heavy", air: true, can_harm: true)},
            { "ShoryEX", new State(a["EXShoryFrames"], "ShoryFalling", 3, hitstop: "Heavy", glow: true, trace: true, air: true, can_be_hit: false, can_harm: true)},
            { "ShoryFalling", new State(a["shoryFallingFrames"], "Landing", change_on_end: false, change_on_ground: true, loop: false, air: true, can_harm: true)},
            { "LightHaduken", new State(a["hadukenFrames"], "Idle", 3, hitstop: "Medium", air: true, can_harm: true)},
            { "HeavyHaduken", new State(a["hadukenFrames"], "Idle", 3, hitstop: "Medium", can_harm: true)},
            { "HadukenEX", new State(a["hadukenFrames"], "Idle", 3, hitstop: "Heavy", glow: true, trace: true, can_harm: true)},
            { "LightTatso", new State(a["lightTatsoFrames"], "Landing", 3, hitstop: "Light", can_harm: true)},
            { "HeavyTatso", new State(a["heavyTatsoFrames"], "Landing", 3, hitstop: "Light", can_harm: true)},
            { "TatsoEX", new State(a["EXTatsoFrames"], "Landing", 3, hitstop: "Light", glow: true, trace: true, can_harm: true)},
            { "AirTatso", new State(a["airTatsoFrames"], "Landing", 3, change_on_ground: true, change_on_end: false, air: true, can_harm: true)},
            { "AirTatsoEX", new State(a["airTatsoFrames"], "Landing", 3, change_on_ground: true, change_on_end: false, glow: true, trace: true, air: true, can_harm: true)},
            // Other
            { "Falling", new State(a["fallingFrames"], "OnGround", on_hit: true, can_be_hit: false)},
            { "Sweeped", new State(a["sweepedFrames"], "Falling", low: true, can_be_hit: false)},
            { "OnGround", new State(a["OnGroundFrames"], "Wakeup", low: true, can_be_hit: false)},
            { "Wakeup", new State(a["wakeupFrames"], "Idle", can_be_hit: false)},
            // Bonus
            { "Intro", new State(a["introFrames"], "Idle", can_be_hit: false)},
        };
    }
    public override void Behave() {
        if (this.behave == false) return;

        if ((this.current_state == "WalkingForward" || this.current_state == "WalkingBackward") & !Input.Key_hold("Left", player: this.player_index, facing: this.facing) & !Input.Key_hold("Right", player: this.player_index, facing: this.facing)) {
            this.ChangeState("Idle");
        }

        // Crouching
        if (Input.Key_hold("Down", player: this.player_index, facing: this.facing) && !Input.Key_hold("Up", player: this.player_index, facing: this.facing) && this.not_acting) {
            this.ChangeState("CrouchingIn");
        }
        if (this.current_state == "Crouching" && !Input.Key_hold("Down", player: this.player_index, facing: this.facing)) {
            this.ChangeState("Idle");
        }

        // Dashing
        if (Input.Key_sequence("Right Right", 10, flexEntry: false, flexTransition: false, player: this.player_index, facing: this.facing) && this.can_dash) {
            this.ChangeState("DashForward");
        } 
        else if (Input.Key_sequence("Left Left", 10, flexEntry: false, flexTransition: false, player: this.player_index, facing: this.facing) && this.can_dash) {
            this.ChangeState("DashBackward");
        }

        // Walking
        if (Input.Key_hold("Left", player: this.player_index, facing: this.facing) && !Input.Key_hold("Right", player: this.player_index, facing: this.facing) && (this.current_state == "Idle" || this.current_state == "WalkingForward" || this.current_state == "WalkingBackward")) {
            this.ChangeState("WalkingBackward");
        } else if (Input.Key_hold("Right", player: this.player_index, facing: this.facing) && !Input.Key_hold("Left", player: this.player_index, facing: this.facing) && (this.current_state == "Idle" || this.current_state == "WalkingBackward" || this.current_state == "WalkingForward")) {
            this.ChangeState("WalkingForward");
        }

        // Jumps
        if (this.not_acting && this.current_anim_frame_index > 0 && Input.Key_hold("Up", player: this.player_index, facing: this.facing) && !Input.Key_hold("Left", player: this.player_index, facing: this.facing) && Input.Key_hold("Right", player: this.player_index, facing: this.facing)) {
            this.ChangeState("JumpForward");
        } else if (this.current_state == "JumpForward" && this.current_anim_frame_index == 1) {
            this.SetVelocity(
                X: 3 + 1, 
                Y: this.jump_hight);
        } 
        else if (this.not_acting && this.current_anim_frame_index > 0 && Input.Key_hold("Up", player: this.player_index, facing: this.facing) && Input.Key_hold("Left", player: this.player_index, facing: this.facing) && !Input.Key_hold("Right", player: this.player_index, facing: this.facing)) {
            this.ChangeState("JumpBackward");
        } else if (this.current_state == "JumpBackward" && this.current_anim_frame_index == 1) {
            this.SetVelocity(
                X: -(3 + 1), 
                Y: this.jump_hight);
        }
        else if (this.not_acting && this.current_anim_frame_index > 0 && Input.Key_hold("Up", player: this.player_index, facing: this.facing)) {
            this.ChangeState("Jump");
            this.SetVelocity(
                X: 0, 
                Y: this.jump_hight);
        }

        // Super
        if (Input.Key_sequence("Down Down RB", 10, player: this.player_index, facing: this.facing) && !this.on_air && (this.not_acting || this.not_acting_low || (this.has_hit && (this.current_state == "CloseMP" || this.current_state.Contains("Shory") || this.current_state == "LowLightK"))) && Character.CheckAuraPoints(this, 100)) {
            Character.UseSuperPoints(this, 100);
            this.ChangeState("SA1");
            this.stage.StopFor(16);
            this.hitstop_counter = 0;
            this.SA_flag = false;

        } else if (this.current_state == "SA1" && this.current_anim_frame_index == 3 && this.has_frame_change) {
            this.stage.spawnParticle("SALighting", this.body.Position.X, this.body.Position.Y, X_offset: 50, Y_offset: -120, facing: this.facing);
            this.stage.StopFor(50);

        } else if (this.current_state == "SA1" && this.current_animation.on_last_frame && this.SA_flag) {
            this.ChangeState("SA1_tail");

        } else if (this.current_state == "SA1_tail" && this.current_anim_frame_index == 3 && this.has_frame_change ) {
            this.AddVelocity(
                X: 1, 
                Y: 150);
        }

        // Shorys
        if (Input.Key_sequence("Right Down Right C", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low || this.has_hit && (this.current_state == "MediumP" || this.current_state == "LightP" || this.current_state == "CloseMP" || this.current_state == "LowLightK" || this.current_state == "LowMediumP"))) {
            this.ChangeState("LightShory");
        } else if (this.current_state == "LightShory" && this.current_anim_frame_index == 3 && this.has_frame_change) {
            this.AddVelocity(
                X: 1.6f, 
                Y: 50);
        } 
        if (Input.Key_sequence("Right Down Right D", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low || this.has_hit && (this.current_state == "LowMediumP" || this.current_state == "LowLightK" ))) {
            this.ChangeState("HeavyShory");
        } else if (this.current_state == "HeavyShory" && this.current_anim_frame_index == 3 && this.has_frame_change) {
            this.AddVelocity(
                X: 5f, 
                Y: 80);
        } 
        if (Input.Key_sequence("Right Down Right RB", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low || this.has_hit && (this.current_state == "LowMediumP" || this.current_state == "LowLightK")) && Character.CheckAuraPoints(this, 50)) {
            Character.UseSuperPoints(this, 50);
            this.ChangeState("ShoryEX");
        } else if (this.current_state == "ShoryEX" && this.current_anim_frame_index == 3 && this.has_frame_change) {
            this.AddVelocity(
                X: 6f, 
                Y: 100);
        } 
        if (this.current_state.Contains("Shory") && this.state.animation.on_last_frame) this.body.Velocity.X = 0;

        // Haduken
        if (this.current_fireball != null && this.current_fireball.remove) this.current_fireball = null;
        if (this.current_fireball == null && Input.Key_sequence("Down Right C", 10, player: this.player_index, facing: this.facing) && ((this.not_acting || this.not_acting_low) || (this.has_hit && (this.current_state == "MediumP" || this.current_state == "LightP" || this.current_state == "LowLightK")))) {
            this.ChangeState("LightHaduken");
        } else if (this.current_state == "LightHaduken" && this.current_anim_frame_index == 3) {
            this.spawnFireball("Ken1", this.body.Position.X, this.body.Position.Y - 5, this.facing, this.player_index, X_offset: 25);
        } 
        if (this.current_fireball == null && Input.Key_sequence("Down Right D", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low)) {
            this.ChangeState("HeavyHaduken");
        } else if (this.current_state == "HeavyHaduken" && this.current_anim_frame_index == 4) {
            this.spawnFireball("Ken2", this.body.Position.X, this.body.Position.Y - 5, this.facing, this.player_index, X_offset: 25);
        }
        if (this.current_fireball == null && Input.Key_sequence("Down Right RB", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low) && Character.CheckAuraPoints(this, 50)) {
            Character.UseSuperPoints(this, 50);
            this.ChangeState("HadukenEX");
        } else if (this.current_state == "HadukenEX" && this.current_anim_frame_index == 4) {
            this.spawnFireball("Ken3", this.body.Position.X, this.body.Position.Y - 5, this.facing, this.player_index, life_points: 2, X_offset: 25);
            this.PlaySound("EX");
        }

        // Tatso
        if (Input.Key_sequence("Down Left A", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low || (this.has_hit && (this.current_state == "LowLightK" || this.current_state == "CloseMP")))) {
            this.ChangeState("LightTatso");
            this.SetVelocity(Y: 5);
        } else if (this.current_state == "LightTatso") {
            this.AddVelocity(Y: 0.5f, raw_set: true);

        } else if (Input.Key_sequence("Down Left B", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low || (this.has_hit && (this.current_state == "LowLightK" || this.current_state == "CloseMP")))) {
            this.ChangeState("HeavyTatso");
            this.SetVelocity(Y: 5);
        } else if (this.current_state == "HeavyTatso") {
            this.AddVelocity(Y: 0.55f, raw_set: true);

        } else if (Input.Key_sequence("Down Left RB", 10, player: this.player_index, facing: this.facing) && (this.not_acting || this.not_acting_low || (this.has_hit && (this.current_state == "LowLightK" || this.current_state == "CloseMP"))) && Character.CheckAuraPoints(this, 50)) {
            Character.UseSuperPoints(this, 50);
            this.ChangeState("TatsoEX");
            this.SetVelocity(Y: 5);
        } else if (this.current_state == "TatsoEX") {
            this.AddVelocity(Y: 0.5f, raw_set: true);
        }
        
        // Air tatso
        if ((Input.Key_sequence("Down Left B", 10, player: this.player_index, facing: this.facing) || Input.Key_sequence("Down Left A", 10, player: this.player_index, facing: this.facing)) && this.not_acting_air) {
            this.ChangeState("AirTatso");
            this.PlaySound("tatso");
        } else if (Input.Key_sequence("Down Left RB", 10, player: this.player_index, facing: this.facing) && this.not_acting_air && Character.CheckAuraPoints(this, 50)) {
            Character.UseSuperPoints(this, 50);
            this.ChangeState("AirTatsoEX");
            this.PlaySound("EX");
            this.PlaySound("tatso");            
        } 

        // Normals plus
        if (Input.Key_sequence("D", Config.hit_stop_time, player: this.player_index, facing: this.facing) && this.has_hit && this.current_state == "LightP") {
            this.SetVelocity();
            this.ChangeState("CloseMP");
        } else if (Input.Key_press("B", player: this.player_index, facing: this.facing) && Input.Key_hold("Left", player: this.player_index, facing: this.facing) && this.not_acting && !this.crounching) {
            this.ChangeState("BackMediumK");
        } else if (Input.Key_press("D", player: this.player_index, facing: this.facing) && Input.Key_hold("Left", player: this.player_index, facing: this.facing) && (this.not_acting || (this.has_hit && this.current_state == "CloseMP"))) {
            this.ChangeState("BackMediumP");
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
                Character.Push(target: target, self: this, Config.light_pushback);
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 3);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 13, 50);
                    target.Stun(this, 6);
                }
                Character.AddAuraPoints(target, this, hit);
                break;
                
            case "LowLightP":
                Character.Push(target: target, self: this, Config.light_pushback);
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 3);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 13, 50);
                    target.Stun(this, 6);
                }
                Character.AddAuraPoints(target, this, hit);
                break;

            case "AirLightP":
                Character.Push(target: target, self: this, Config.light_pushback, force_push: true);
                if (target.isBlockingHigh()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 7);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 50, 100);
                    target.Stun(this, 7);
                }
                Character.AddAuraPoints(target, this, hit);
                break;

            case "LightK":
                Character.Push(target: target, self: this, Config.light_pushback);
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 2);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 44, 50);
                    target.Stun(this, 2);
                }
                Character.AddAuraPoints(target, this, hit);
                break;
                
            case "LowLightK":
                Character.Push(target: target, self: this, Config.light_pushback);
                if (target.isBlockingLow()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -4);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 85, 50);
                    target.Stun(this, -3);
                }
                Character.AddAuraPoints(target, this, hit);
                break;
            
            case "AirLightK":
                Character.Push(target: target, self: this, Config.light_pushback, force_push: true);
                if (target.isBlockingHigh()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 10);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 90, 150);
                    target.Stun(this, 10);
                }
                Character.AddAuraPoints(target, this, hit);
                break;

            case "MediumP":
                Character.Push(target: target, self: this, Config.medium_pushback);
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -2);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 100, 172);
                    target.Stun(this, 0);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "LowMediumP":
                Character.Push(target: target, self: this, Config.medium_pushback);
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 3);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 90, 100);
                    target.Stun(this, 4);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "AirMediumP":
                Character.Push(target: target, self: this, Config.medium_pushback, force_push: true);
                if (target.isBlockingHigh()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 10);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 100, 180);
                    target.Stun(this, 10);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "MediumK":
                Character.Push(target: target, self: this, Config.medium_pushback);
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -6);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 120, 235);
                    target.Stun(this, 0);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "LowMediumK":
                Character.Push(target: target, self: this, Config.medium_pushback);
                if (target.isBlockingLow()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -14);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 110, 200);
                    target.Stun(target, 0, sweep: true);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "AirMediumK":
                Character.Push(target: target, self: this, Config.medium_pushback, force_push: true);
                if (target.isBlockingHigh()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 13);
                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 125, 235);
                    target.Stun(this, 13);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "BackMediumK":
                Character.Push(target: target, self: this, Config.heavy_pushback);
                if (target.isBlockingHigh()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 1);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 56, 94);
                    target.Stun(this, 10);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 6);
                break;

            case "BackMediumP":
                Character.Push(target: target, self: this, Config.heavy_pushback);
                if (target.isBlockingHigh()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 1);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 56, 94);
                    target.Stun(this, 10);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "CloseMP":
                Character.Push(target: target, self: this, Config.medium_pushback);
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -2);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 80, 100);
                    target.Stun(this, 0);
                }
                Character.AddAuraPoints(target, this, hit);
                break;
            
            case "LightShory":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -10);
                    Character.Push(target: target, self: this, Config.light_pushback);

                } else {
                    hit = Character.HIT;
                    target.Stun(this, 0, airbone: true);
                    Character.Push(target: target, self: this, Config.light_pushback, Y_amount: 110f, airbone: true);
                    Character.Damage(target: target, self: this, 80, 160);
                }
                Character.AddAuraPoints(target, this, hit);
                break;

            case "HeavyShory":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -17);
                    Character.Push(target: target, self: this, Config.medium_pushback);

                } else {
                    hit = Character.HIT;
                    if (this.current_anim_frame_index >= 3) {
                        target.Stun(this, 0, airbone: true);
                        Character.Push(target: target, self: this, Config.medium_pushback, Y_amount: 100f, airbone: true);

                    } else target.Stun(this, 0);

                    Character.Damage(target: target, self: this, 100, 85);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "ShoryEX":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -10);
                    Character.Push(target: target, self: this, Config.medium_pushback);
                    Character.Damage(target: target, self: this, 20, 20);

                } else {
                    hit = Character.HIT;
                    if (this.current_anim_frame_index >= 3) {
                        target.Stun(this, 0, airbone: true);
                        Character.Push(target: target, self: this, Config.medium_pushback, Y_amount: 110f, airbone: true);

                    } else target.Stun(this, 0);
                    
                    Character.Damage(target: target, self: this, 120, 85);

                }
                Character.AddAuraPoints(target, this, hit, self_amount: 2);
                break;
            
            case "LightTatso":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
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
                    target.BlockStun(this, 6, raw_value: true);
                    Character.Damage(target: target, self: this, 10, 30);
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
                    target.BlockStun(this, 1);

                } else {
                    hit = Character.HIT;
                    target.Stun(this, -6, airbone: true);
                    Character.Push(target: target, self: this, 2f, Y_amount: 50, airbone: true);
                    Character.Damage(target: target, self: this, 80, 140);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 4);
                break;

            case "SA1":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 30, raw_value: true);
                    Character.Push(target: target, self: this, Config.heavy_pushback);
                    Character.Damage(target: target, self: this, 5, 0);
                } else {
                    hit = Character.HIT;
                    target.Stun(this, 10, force_stand: true);
                    Character.Push(target: target, self: this, 1, Y_amount: 20);
                    Character.Damage(target: target, self: this, 45, 35);
                    this.SA_flag = true;
                }
                break;
                
            case "SA1_tail":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    Character.Damage(target: target, self: this, 5, 0);
                    target.BlockStun(this, 30, raw_value: true);

                } else {
                    hit = Character.HIT;
                    target.Stun(this, 5, airbone: true);

                    if (this.current_anim_frame_index < 6) {
                        Character.Push(target: target, self: this, 1, Y_amount: 150, airbone: true);
                    } else if (this.current_anim_frame_index >= 13) {
                        Character.Push(target: target, self: this, Config.heavy_pushback, Y_amount: 80, airbone: true);
                    }

                    Character.Damage(target: target, self: this, 45, 35);
                }
                Character.AddAuraPoints(target, this, hit, self_amount: 2);
                break;
                
            case "Shungoku":
                this.stage.spawnParticle("Shungoku", target.body.Position.X, this.body.Position.Y, Y_offset: -125, facing: this.facing);
                this.stage.spawnParticle("Shungoku_text", Camera.X, Camera.Y);
                this.stage.StopFor(40 * 4); // 40 frames a 15 fps

                this.ChangeState("Shungoku_End");
                this.SetVelocity();
                this.body.Position.X = target.body.Position.X - 100 * this.facing;

                target.SetVelocity();
                target.ChangeState("OnGround");

                Character.Damage(target: target, self: this, 400, 0);
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
                this.BOT.EnqueueMove("Left", 10);
            } else if (choise <= 5) {
                this.BOT.EnqueueMove("Left", 20);
                this.BOT.EnqueueMove("Right", 10);
            } else if (choise == 10) {
                this.BOT.EnqueueMove("Right *", 2);
                this.BOT.EnqueueMove("*", 2);
                this.BOT.EnqueueMove("Right *", 2);
            } else
                this.BOT.EnqueueMove("", 5);

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

            } else 
                this.BOT.EnqueueMove("", 10);

        } else {
            var choise = AI.rand.Next(0, 10);
            if (choise == 1) { 
                this.BOT.EnqueueMove("Down", 20);
            } else if (choise < 6) {
                this.BOT.EnqueueMove("Right", 20);
            } else 
                this.BOT.EnqueueMove("", 60);
        } 

    }
    public override void SelectAction(FightState f_state) {
        // return;
        if (f_state.enemyIsDead) return;
        if (this.state.can_harm && !this.on_hit && !this.state.on_block && !this.state.on_parry) return;

        if (this.on_hit || this.state.on_block || (f_state.enemyIsAttacking && f_state.enemyDistance <= 0.5f)) { // Block
            if (f_state.enemyIsCrouching) this.BOT.EnqueueAction("Left Down *", 5);
            else this.BOT.EnqueueAction("Left *", 5);

            return;
        }
        
        if (f_state.enemyIsAirborne && f_state.enemyDistance <= 0.3f) { // Anti air
            var choise = AI.rand.Next(0, 4);
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

                } else if (choise < 4) { // Low LK
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

                } else if (choise < 5) { // Sweep
                    this.BOT.EnqueueAction("Down *", 5);
                    this.BOT.EnqueueAction("Down B *", 5);

                } else if (choise < 6) { // Light Shory
                    this.BOT.EnqueueAction("Right *", 5);
                    this.BOT.EnqueueAction("Down *", 5);
                    this.BOT.EnqueueAction("Right *", 5);
                    this.BOT.EnqueueAction("C", 1);

                } else if (choise < 7) { // Tatso                
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

            return;

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
    
        this.BOT.EnqueueAction("", AI.rand.Next(5, 10));
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
}