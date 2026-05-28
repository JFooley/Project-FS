using SFML.System;
using System.IO.Compression;
using SFML.Graphics;
using SFML.Audio;
using UI_space;
using System.Runtime.InteropServices;

public class Remy : Character {
    private static Dictionary<string, Texture> _shared_textures = new Dictionary<string, Texture>();
    public override Dictionary<string, Texture> textures {get => _shared_textures; protected set => _shared_textures = value ?? new Dictionary<string, Texture>();}
    
    private static Dictionary<string, SoundBuffer> _shared_sounds = new Dictionary<string, SoundBuffer>();
    public override Dictionary<string, SoundBuffer> sounds {get => _shared_sounds; protected set => _shared_sounds = value ?? new Dictionary<string, SoundBuffer>();}
    
    private static Dictionary<string, Frame[]> _shared_animations = new Dictionary<string, Frame[]>();
    public override Dictionary<string, Frame[]> animations { get => _shared_animations; protected set => _shared_animations = value ?? new Dictionary<string, Frame[]>();}

    private static Texture? _shared_palette;
    public override Texture palette {get => _shared_palette ?? new Texture(Data.textures["other:placeholder"]); protected set => _shared_palette = value;}

    private bool can_sp1 => this.not_acting_all || this.current_state == "Special1Landing" && this.current_anim_frame_index >= 3;

    // Constructors
    public Remy(string initialState, int startX, int startY)
        : base("Remy", initialState, startX, startY, Data.GetPath("assets/characters/Remy")) {
        this.life_points = new Vector2i(1000, 1000);
        this.dizzy_points = new Vector2i(500, 500);
        this.aura_points = new Vector2i(0, 100);
    } 
    public Remy() : base("Remy", "Idle", Data.GetPath("assets/characters/Remy")) { }
    public override Character Copy() {
        var obj = new Remy("Intro", 0, 0);
        obj.Load();
        return obj;
    }
    public override void Load() {
        var fb = new RemyEffects();
        fb.LoadTextures();
        fb.LoadSounds();
        fb.LoadAnimations();
        fb.Load();

        var a = this.animations;

        this.states = new Dictionary<string, State> {
            { "Intro", new State(F(a["intro1"], a["intro2"]), "Idle", can_be_hit: false)},
            { "Win", new State(F(a["win1"]), "Idle", change_on_end: false, loop: false)},
            // Basic
            { "Idle", new State(F(a["idle"]), "Idle", idle: true)},
            { "OnBlock", new State(F(a["onBlock"]), "Idle", on_block: true)}, 
            { "OnBlockLow", new State(F(a["onBlockLow"]), "Crouching", low: true, on_block: true)},
            { "OnHit", new State(F(a["onHit1"], a["onHit2"]), "Idle", on_hit: true)},
            { "OnHitLow", new State(F(a["onHitLow1"], a["onHitLow2"]), "Crouching", low: true, on_hit: true)},
            { "Airboned", new State(F(a["airboned"]), "Falling", change_on_ground: true, change_on_end: false, loop: false, air: true, on_hit: true)},
            { "Parry", new State(F(a["parry"]), "Idle", 6, glow: true, is_parry: true)},
            { "AirParry", new State(F(a["airParry"]), "JumpFalling", 6, glow: true, air: true, is_parry: true)},
            // Normals
            { "LightP", new State(F(a["LP"]), "Idle", 0, will_hit: true)},
            { "LowLightP", new State(F(a["lowLP"]), "Crouching", 0, low: true, will_hit: true)},
            { "AirLightP", new State(F(a["airLP"]), "Landing", 0, change_on_end: false, change_on_ground: true, loop: false, air: true, will_hit: true)},
            { "FrontLightP", new State(F(a["frontLP"]), "Idle", 0, will_hit: true)},
            { "LightK", new State(F(a["LK"]), "Idle", 0, will_hit: true)},
            { "LowLightK", new State(F(a["lowLK"]), "Crouching", 1, hitstop: "Medium", low: true, will_hit: true)},
            { "AirLightK", new State(F(a["airLK"]), "Landing", 0, change_on_end: false, change_on_ground: true, loop: false, air: true, will_hit: true)},
            { "MediumP", new State(F(a["MP"]), "Idle", 1, hitstop: "Medium", will_hit: true)},
            { "LowMediumP", new State(F(a["lowMP"]), "Crouching", 1, hitstop: "Medium", low: true, will_hit: true)},
            { "AirMediumP", new State(F(a["airMP"]), "Landing", 1, hitstop: "Medium", change_on_end: false, change_on_ground: true, loop: false, air: true, will_hit: true)},
            { "MediumK", new State(F(a["MK"]), "Idle", 1, hitstop: "Medium", will_hit: true)},
            { "LowMediumK", new State(F(a["lowMK"]), "Crouching", 2, hitstop: "Light", low: true, will_hit: true)},
            { "AirMediumK", new State(F(a["airMK"]), "Landing", 1, hitstop: "Medium", change_on_end: false, change_on_ground: true, loop: false, air: true, will_hit: true)},
            { "FrontMediumK", new State(F(a["frontMK"]), "Idle", 1, hitstop: "Medium", will_hit: true)},
            // Throw
            { "Throw", new State(F(a["throw"]), "Idle", 5, will_hit: true, is_grab: true, hitstop: "")},
            { "ThrowLeft", new State(F(a["throw_left"]), "Idle", 5, will_hit: true, is_grab: true, hitstop: "")},
            { "ThrowRight", new State(F(a["throw_right"]), "Idle", 4, will_hit: true, is_grab: true, hitstop: "")},
            { "OnThrowRight_KEN", new State(F(a["on_throw_right_KEN"]), "Falling", 0, has_gravity: false, on_hit: true)},
            { "OnThrowLeft_KEN", new State(F(a["on_throw_left_KEN"]), "Airboned", 0, has_gravity: false, on_hit: true)},
            { "OnThrow_REMY", new State(F(a["on_throw_REMY"]), "Falling", 0, has_gravity: false, on_hit: true)},
            // Movement
            { "WalkingForward", new State(F(a["walkingForward"]), "WalkingForward", idle: true)},
            { "WalkingBackward", new State(F(a["walkingBackward"]), "WalkingBackward", idle: true)},
            { "DashIn", new State(F(a["dashIn"]), "Idle", 20)},
            { "DashOut", new State(F(a["dashOut"]), "Idle", 20)},
            { "Jump", new State(F(a["jump"]), "JumpFalling", idle: true, air: true)},
            { "JumpForward", new State(F(a["jump"]), "JumpFalling", idle: true, air: true)}, 
            { "JumpBackward", new State(F(a["jumpBackward"]), "JumpFalling", idle: true, air: true)},
            { "JumpFalling", new State(F(a["jumpFalling"]), "Landing", idle: true, change_on_end: false, change_on_ground: true, loop: false, air: true)},
            { "Landing", new State(F(a["landing"]), "CrouchingOut", idle: true)},
            { "CrouchingIn", new State(F(a["crouchingIn"]), "Crouching", idle: true, low: true)},
            { "CrouchingOut", new State(F(a["crouchingOut"]), "Idle", idle: true)},
            { "Crouching", new State(F(a["crouching"]), "Crouching", idle: true, low: true)},
            // Super
            { "SA1", new State(F(a["SA1"]), "Idle", 4, trace: true, drama_wait: true, will_hit: true, hitstop: "")},
            // Specials special1_falling
            { "LightSpecial1", new State(F(a["special1"]), "Special1Falling", 3, hitstop: "Light", air: true, will_hit: true)},
            { "MediumSpecial1", new State(F(a["special1"]), "Special1Falling", 3, hitstop: "Medium", air: true, will_hit: true)},
            { "EXSpecial1", new State(F(a["ex_special1"]), "Special1Falling", 3, hitstop: "Heavy", air: true, will_hit: true, can_be_hit: false, glow: true, trace: true)},
            { "Special1Falling", new State(F(a["jumpFalling"]), "Special1Landing", change_on_end: false, change_on_ground: true, loop: false, air: true)},
            { "Special1Landing", new State(F(a["landing"]), "CrouchingOut")},
            { "Special2Punch", new State(F(a["special2_punch"]), "Idle", 3, air: true, will_hit: true)},
            { "Special2Kick", new State(F(a["special2_kick"]), "Idle", 3, air: true, will_hit: true)},
            { "Special2EX", new State(F(a["special2_ex"]), "Idle", 3, air: true, will_hit: true, glow: true, trace: true)},
            // Other
            { "Falling", new State(F(a["falling"]), "OnGround", on_hit: true, can_be_hit: false)},
            { "Sweeped", new State(F(a["sweeped"]), "Falling", on_hit: true, low: true, can_be_hit: false)},
            { "OnGround", new State(F(a["onGround"]), "WakeUp", low: true, can_be_hit: false)},
            { "WakeUp", new State(F(a["wakeUp"]), "Idle", can_be_hit: false)},
        };
    }
    public override void Behave() {
        if (this.behave == false) return;
        // Super
        if (Input.Key_sequence("Down Down RB", 10, player: this.player_index, facing: this.facing) && !this.on_air && (this.not_acting || this.not_acting_low || (this.has_hit && (this.current_state == "CloseMP" || this.current_state.Contains("Shory") || this.current_state == "LowLightK"))) && Character.CheckAuraPoints(this, 100)) {
            Character.UseAuraPoints(this, 100);
            this.ChangeState("SA1");
            this.stage.StopFor(9, 0, this);
            return;
        } else if (this.current_state == "SA1") {
            if (this.current_anim_frame_index == 3 && this.has_frame_changed) {
                this.stage.PSSuper("SABlink", this.body.position.X, this.body.position.Y, X_offset: 50, Y_offset: -120, facing: this.facing);
                this.stage.StopFor(50);
            } else if (this.current_anim_frame_index == 4 && this.has_frame_changed) {
                this.SpawnEffect("FireballEXHigh", this.body.position.X, this.body.position.Y, this.facing, X_offset: 30, Y_offset: -75);
                this.SpawnEffect("FireballEXHigh", this.body.position.X, this.body.position.Y, this.facing, X_offset: 50, Y_offset: -65);
                this.SpawnEffect("FireballEXHigh", this.body.position.X, this.body.position.Y, this.facing, X_offset: 70, Y_offset: -60);
            } else if (this.current_anim_frame_index == 13 && this.has_frame_changed) {
                this.SpawnEffect("FireballEXLow", this.body.position.X, this.body.position.Y, this.facing, X_offset: 50, Y_offset: -10);
                this.SpawnEffect("FireballEXLow", this.body.position.X, this.body.position.Y, this.facing, X_offset: 30, Y_offset: 0);
                this.SpawnEffect("FireballEXLow", this.body.position.X, this.body.position.Y, this.facing, X_offset: 70, Y_offset: -20);
            } else if (this.current_anim_frame_index == 24 && this.has_frame_changed) {
                this.SpawnEffect("FireballEXHigh", this.body.position.X, this.body.position.Y, this.facing, X_offset: 30, Y_offset: -60);
                this.SpawnEffect("FireballEXLow", this.body.position.X, this.body.position.Y, this.facing, X_offset: 50, Y_offset: 0);
                this.SpawnEffect("FireballEXHigh", this.body.position.X, this.body.position.Y, this.facing, X_offset: 70, Y_offset: -50);
            }

        }

        // Specials
        if (Input.Key_sequence("Down Up A", 10, player: this.player_index, facing: this.facing) && this.can_sp1) {
            this.ChangeState("LightSpecial1");
            this.SpawnEffect("Cut", this.body.position.X, this.body.position.Y, this.facing);
            this.SetVelocity(
                X: 1.0f,
                Y: 50);
            return;
        } 
        else if (Input.Key_sequence("Down Up B", 10, player: this.player_index, facing: this.facing) && this.can_sp1) {
            this.ChangeState("MediumSpecial1");
            this.SpawnEffect("Cut", this.body.position.X, this.body.position.Y, this.facing);
            this.SpawnEffect("Dust", this.body.position.X, this.floor_line, this.facing, X_offset: 50, Y_offset: 0);
            this.SetVelocity(
                X: 2.0f,
                Y: 60);
            return;
        } 
        else if (Input.Key_sequence("Down Up RB", 10, player: this.player_index, facing: this.facing) && this.can_sp1) {
            Character.UseAuraPoints(this, 50);
            this.PlaySound("generic:ex");
            this.ChangeState("EXSpecial1");
            this.SpawnEffect("Cut", this.body.position.X, this.body.position.Y, this.facing);
            this.SpawnEffect("Dust", this.body.position.X, this.floor_line, this.facing, X_offset: 50, Y_offset: 0);
            this.SetVelocity(
                X: 3.0f,
                Y: 80);
            return;
        } 

        if (this.not_acting && (Input.Key_sequence("Left Right C", 10, this.player_index, this.facing) || Input.Key_sequence("Left Right D", 10, this.player_index, this.facing))) {
            this.ChangeState("Special2Punch");
            return;
        } else if (this.current_state == "Special2Punch" && this.current_anim_frame_index == 4 && this.has_frame_changed) {
            this.SpawnEffect("FireballHigh", this.body.position.X, this.body.position.Y, this.facing, X_offset: 35, Y_offset: -57);
        }
        else if (this.not_acting && (Input.Key_sequence("Left Right A", 10, this.player_index, this.facing) || Input.Key_sequence("Left Right B", 10, this.player_index, this.facing))) {
            this.ChangeState("Special2Kick");
            return;
        } else if (this.current_state == "Special2Kick" && this.current_anim_frame_index == 3 && this.has_frame_changed) {
            this.SpawnEffect("FireballLow", this.body.position.X, this.body.position.Y, this.facing, X_offset: 35, Y_offset: 0);
        }
        else if (this.not_acting && Input.Key_sequence("Left Right RB", 10, this.player_index, this.facing)) {
            Character.UseAuraPoints(this, 50);
            this.ChangeState("Special2EX");
            this.PlaySound("generic:ex");
            return;
        } else if (this.current_state == "Special2EX" && this.current_anim_frame_index == 5 && this.has_frame_changed) {
            this.SpawnEffect("FireballHigh", this.body.position.X, this.body.position.Y, this.facing, X_offset: 30, Y_offset: -57);
            this.SpawnEffect("FireballLow", this.body.position.X, this.body.position.Y, this.facing, X_offset: 50, Y_offset: 0);
            this.SpawnEffect("FireballDizzy", this.body.position.X, this.body.position.Y, this.facing, X_offset: 70, Y_offset: -47);
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
        if (this.not_acting && Input.Key_press("C", player: this.player_index, facing: this.facing, input_window: Config.hit_stop_time) && Input.Key_hold("Right", player: this.player_index, facing: this.facing)) {
            this.ChangeState("FrontLightP");
            return;
        } else if ((this.not_acting || this.has_hit && this.current_state == "LightK") && Input.Key_press("B", player: this.player_index, facing: this.facing, input_window: Config.hit_stop_time) && Input.Key_hold("Right", player: this.player_index, facing: this.facing)) {
            this.ChangeState("FrontMediumK");
            return;
        }

        // Cancels
        if (this.has_hit && (this.current_state == "FrontLightP" || this.current_state == "LightP") && Input.Key_press("D", this.player_index, this.facing)) {
            this.ChangeState("MediumP");
            return;
        }
        if (this.has_hit && this.current_state == "LowLightK" && Input.Key_press("B", this.player_index, this.facing)) {
            this.ChangeState("LowMediumK");
            return;
        }
        if (this.has_hit && this.current_state == "LowLightP" && Input.Key_press("D", this.player_index, this.facing)) {
            this.ChangeState("LowMediumP");
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
    
        // Crouching
        if (Input.Key_hold("Down", player: this.player_index, facing: this.facing) && !Input.Key_hold("Up", player: this.player_index, facing: this.facing) && this.not_acting) {
            this.ChangeState("CrouchingIn");
        }
        if (this.current_state == "Crouching" && !Input.Key_hold("Down", player: this.player_index, facing: this.facing)) {
            this.ChangeState("CrouchingOut");
        }

        // Dashing
        if (Input.Key_sequence("Right Right", 10, flexEntry: false, flexTransition: false, player: this.player_index, facing: this.facing) && this.can_dash) {
            this.ChangeState("DashIn");
            return;
        } 
        else if (Input.Key_sequence("Left Left", 10, flexEntry: false, flexTransition: false, player: this.player_index, facing: this.facing) && this.can_dash) {
            this.ChangeState("DashOut");
            return;
        }

        // Walking
        if (Input.Key_hold("Left", player: this.player_index, facing: this.facing) && !Input.Key_hold("Right", player: this.player_index, facing: this.facing) && this.not_acting && this.current_state != "WalkingBackward") {
            this.ChangeState("WalkingBackward");
        } else if (Input.Key_hold("Right", player: this.player_index, facing: this.facing) && !Input.Key_hold("Left", player: this.player_index, facing: this.facing) && this.not_acting && this.current_state != "WalkingForward") {
            this.ChangeState("WalkingForward");
        }
        if (this.current_state == "WalkingForward" && !Input.Key_hold("Right", player: this.player_index, facing: this.facing) || (this.current_state == "WalkingBackward" && !Input.Key_hold("Left", player: this.player_index, facing: this.facing))) {
            this.ChangeState("Idle");
        }
        
        // Jumps
        if (this.can_jump && Input.Key_hold("Up", player: this.player_index, facing: this.facing) && !Input.Key_hold("Left", player: this.player_index, facing: this.facing) && Input.Key_hold("Right", player: this.player_index, facing: this.facing)) {
            this.ChangeState("JumpForward");
            return;
        } else if (this.current_state == "JumpForward" && this.current_anim_frame_index == 1 && this.has_frame_changed) {
            this.SetVelocity(
                X: 4, 
                Y: this.jump_hight);
        } 
        else if (this.can_jump && Input.Key_hold("Up", player: this.player_index, facing: this.facing) && Input.Key_hold("Left", player: this.player_index, facing: this.facing) && !Input.Key_hold("Right", player: this.player_index, facing: this.facing)) {
            this.ChangeState("JumpBackward");
            return;
        } else if (this.current_state == "JumpBackward" && this.current_anim_frame_index == 1 && this.has_frame_changed) {
            this.SetVelocity(
                X: -4, 
                Y: this.jump_hight);
        }
        else if (this.can_jump && Input.Key_hold("Up", player: this.player_index, facing: this.facing)) {
            this.ChangeState("Jump");
            this.SetVelocity(X: 0, Y: this.jump_hight);
            return;
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
                    Character.Damage(target: target, self: this, 20, 50);
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
                    Character.Damage(target: target, self: this, 35, 50);
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
                    Character.Damage(target: target, self: this, 70, 50);
                    target.Stun(this, 2);
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
                    Character.Damage(target: target, self: this, 120, 172);
                    target.Stun(this, 2);
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
                    Character.Damage(target: target, self: this, 60, 100);
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
                    target.Stun(this, 2);
                }
                Character.Push(target: target, self: this, Config.medium_pushback);
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;

            case "LowMediumK":
                if (target.isBlockingLow()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -10);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 60, 100);
                    target.Stun(this, -10, sweep: this.current_anim_frame_index >= 12 ? true : false);
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


            case "FrontMediumK":
                if (target.isBlockingHigh()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -2);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 70, 94);
                    target.Stun(this, 10);
                }
                Character.Push(target: target, self: this, Config.heavy_pushback);
                Character.AddAuraPoints(target, this, hit, self_amount: 6);
                break;

            case "FrontLightP":
                if (target.isBlockingHigh()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, 0);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 45, 94);
                    target.Stun(this, 10);
                }
                Character.Push(target: target, self: this, Config.heavy_pushback);
                Character.AddAuraPoints(target, this, hit, self_amount: 16);
                break;


            case "Throw":
                if (target.on_air) break;

                target.SetVelocity(raw_set: true);
                target.SetAcceleration();
                target.body.position = new Vector2f(this.body.position.X + (this.facing * 55), this.body.position.Y);
                target.ChangeState("OnThrow_REMY");
                
                if (Input.Key_hold("Left", this.player_index, this.facing)) {
                    this.ChangeState("ThrowLeft");
                } else {
                    this.ChangeState("ThrowRight");
                }
                
                hit = Character.GRAB;
                break;
            
            case "ThrowLeft":
                target.body.velocity = new Vector2f(7 * -this.facing, 5);
                target.Stun(this, 0, airbone: true);
                Character.Damage(target: target, self: this, 100, 203);
                Character.AddAuraPoints(target, this, hit, self_amount: 10);
                break;

            case "ThrowRight":
                Character.Damage(target: target, self: this, 40, 50);
                Character.AddAuraPoints(target, this, hit, self_amount: 10);
                if (this.current_anim_frame_index >= 18) {
                    target.Stun(this, 0, airbone: true);
                    Character.Push(target: target, self: this, 0.5f, Y_amount: 100f, airbone: true, fixed_height: true);
                } else {
                    target.Stun(this, 0, force_stand: true);
                }
                hit = Character.HIT;
                break;


            case "LightSpecial1":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -10);
                    Character.Push(target: target, self: this, 2.5f);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 80, 160);
                    target.Stun(this, 0, airbone: true);
                    Character.Push(target: target, self: this, 2.5f, Y_amount: 100f, airbone: true, fixed_height: true);
                }
                Character.AddAuraPoints(target, this, hit);
                break;
            
            case "MediumSpecial1":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    Character.Damage(target: target, self: this, 10, 18);
                    target.BlockStun(this, -15);
                    Character.Push(target: target, self: this, 3.5f);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 100, 180);
                    target.Stun(this, 0, airbone: true);
                    Character.Push(target: target, self: this, 3.5f, Y_amount: 120f, airbone: true, fixed_height: true);
                }
                Character.AddAuraPoints(target, this, hit);
                break;
            
            case "EXSpecial1":
                if (target.isBlocking()) {
                    hit = Character.BLOCK;
                    target.BlockStun(this, -10);
                    Character.Push(target: target, self: this, 4.0f);

                } else {
                    hit = Character.HIT;
                    Character.Damage(target: target, self: this, 50, 60);
                    target.Stun(this, 0, airbone: true);
                    Character.Push(target: target, self: this, 4.0f, Y_amount: 140f, airbone: true, fixed_height: true);
                }
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
        this.BOT.EnqueueMove("", AI.rand.Next(5 * this.BOT.difficulty, 20 * this.BOT.difficulty));
    }
    public override void SelectAction(FightState f_state) {
        if (f_state.enemyIsDead) return;

        // Follow up
        if (this.current_state == "ThrowRight" && this.current_animation.on_last_frame && Character.CheckAuraPoints(this, 50)) {
            this.BOT.EnqueueAction("Down", 5);
            this.BOT.EnqueueAction("Up RB", 5);
            return;
        } else if ((f_state.enemyDistance <= 0.3f && f_state.enemyIsAirborne && !f_state.enemyIsOnHit) || (this.has_hit && this.state.air) || this.current_state == "LightP") {
            var choise = AI.rand.Next(0, this.BOT.difficulty + 2);
            if (choise == 0) { // Facão
                this.BOT.EnqueueAction("Down *", 10);
                this.BOT.EnqueueAction("Up *", 2);

                choise = AI.rand.Next(0, 2);
                if (Character.CheckAuraPoints(this, 50)) 
                    this.BOT.EnqueueAction("RB *", 5);
                else if (choise == 0) 
                    this.BOT.EnqueueAction("B *", 5);
                else 
                    this.BOT.EnqueueAction("A *", 5);
                
                this.BOT.EnqueueAction("", 20);

            } else if (choise == 1 && Character.CheckAuraPoints(this, 100)) { // Super
                this.BOT.EnqueueAction("Down *", 5);
                this.BOT.EnqueueAction("Down RB *", 5);
            } 
        } 

        // Block
        if (this.on_hit || this.state.on_block || ((f_state.enemyIsAttacking || f_state.enemyIsGrabbing) && f_state.enemyDistance <= 0.5f)) {
            if (f_state.enemyIsGrabbing) this.BOT.EnqueueAction("LB *", 5);
            else if (f_state.enemyIsCrouching) this.BOT.EnqueueAction("Left Down *", 5);
            else this.BOT.EnqueueAction("Left *", 5);
            return;
        }

        // Atack
        if (this.state.air && f_state.enemyDistance < 0.5f) { // Air
            var choise = AI.rand.Next(0, 4);
            if (choise == 0) {
                this.BOT.EnqueueAction("A", 3);
            } else if (choise == 1) {
                this.BOT.EnqueueAction("B", 3);
            } else if (choise == 2) {
                this.BOT.EnqueueAction("C", 3);
            } else {
                this.BOT.EnqueueAction("D", 3);
            }
            
        } else if (f_state.enemyDistance < 0.3f) { // Close range
            var choise = AI.rand.Next(0, 5);
            if (this.crounching || choise == 0) { // LOW
                this.BOT.EnqueueAction("Down *", 3);

                choise = AI.rand.Next(0, 10);

                if (choise < 2) { 
                    this.BOT.EnqueueAction("Down A *", 3);

                } else if (choise < 5) {
                    this.BOT.EnqueueAction("Down D *", 3);

                } else if (choise < 8) { // Low LP
                    this.BOT.EnqueueAction("Down C *", 3);
                    
                } else { // Sweep
                    this.BOT.EnqueueAction("Down *", 5);
                    this.BOT.EnqueueAction("Down B *", 5);
                } 

            } else if (choise == 1) { // Special
                choise = AI.rand.Next(0, 5);
                if (choise == 0) { // Target combo 1
                    this.BOT.EnqueueAction("Right C *", 5);
                    this.BOT.EnqueueAction("D *", 5);

                } else if (choise == 1) { // Facão
                    this.BOT.EnqueueAction("Down *", 10);
                    this.BOT.EnqueueAction("Up *", 5);

                    choise = AI.rand.Next(0, 2);
                    if (Character.CheckAuraPoints(this, 50)) this.BOT.EnqueueAction("RB *", 5);
                    else if (choise == 1) this.BOT.EnqueueAction("B *", 5);
                    else this.BOT.EnqueueAction("A *", 5);

                } else if (choise == 2) { // Target combo 2
                    this.BOT.EnqueueAction("Down *", 10);
                    this.BOT.EnqueueAction("Down C *", 5);
                    this.BOT.EnqueueAction("Down D *", 5);

                } else if (choise == 3) { // Target combo 3
                    this.BOT.EnqueueAction("Down *", 10);
                    this.BOT.EnqueueAction("Down A *", 5);
                    this.BOT.EnqueueAction("Down B *", 5);

                } else if (Character.CheckAuraPoints(this, 100)) { // Super
                    this.BOT.EnqueueAction("C *", 5);
                    this.BOT.EnqueueAction("Down *", 3);
                    this.BOT.EnqueueAction("Down RB *", 3);
                }

            } else { // STAND
                choise = AI.rand.Next(0, 10);
                if (choise < 3) {
                    this.BOT.EnqueueAction("A", 3);
                } else if (choise < 5) {
                    this.BOT.EnqueueAction("B", 3);
                } else if (choise < 7) {
                    this.BOT.EnqueueAction("C", 3);
                } else if (choise < 9) {
                    this.BOT.EnqueueAction("D", 3);
                } else { // Grab
                    choise = AI.rand.Next(0, 2);
                    if (choise == 0) this.BOT.EnqueueAction("LB *", 5);
                    else this.BOT.EnqueueAction("LB Left *", 5);
                }
            }

        } else if (f_state.enemyDistance < 0.4f) { // Mid range
            var choise = AI.rand.Next(0, 10);
            if (choise < 1) { // MK
                choise = AI.rand.Next(0, 2);
                if (choise == 0) this.BOT.EnqueueAction("B", 5);
                else this.BOT.EnqueueAction("Down B", 10);

            } else if (choise < 2) { // Low LK
                this.BOT.EnqueueAction("Right *", 20);
                this.BOT.EnqueueAction("Down A *", 5);

                if (AI.rand.Next(0, 10) < 3) { // Follow up
                    this.BOT.EnqueueAction("B", 5);
                }
            
            } else if (choise < 3) { // Fireball
                this.BOT.EnqueueAction("Left *", 10);
                this.BOT.EnqueueAction("Right *", 5);

                choise = AI.rand.Next(0, 3);
                if (choise == 0 && Character.CheckAuraPoints(this, 50)) 
                    this.BOT.EnqueueAction("RB", 5);
                else if (choise == 1) 
                    this.BOT.EnqueueAction("C", 5);
                else 
                    this.BOT.EnqueueAction("A", 5);

            } else if (choise < 4) { // Facão
                this.BOT.EnqueueAction("Down *", 10);
                this.BOT.EnqueueAction("Up *", 5);

                choise = AI.rand.Next(0, 3);
                if (choise == 0 && Character.CheckAuraPoints(this, 50)) this.BOT.EnqueueAction("RB *", 5);
                else if (choise == 1) this.BOT.EnqueueAction("B *", 5);
                else this.BOT.EnqueueAction("A *", 5);

            } else if (choise == 0 && Character.CheckAuraPoints(this, 100)) { // Super
                this.BOT.EnqueueAction("Down *", 5);
                this.BOT.EnqueueAction("Down *", 5);
                this.BOT.EnqueueAction("RB *", 5);
            }

        } else { // Long range
            var choise = AI.rand.Next(0, 25);
            if (choise <= 5) {  // Fireball
                this.BOT.EnqueueAction("Left *", 10);
                this.BOT.EnqueueAction("Right *", 5);

                choise = AI.rand.Next(0, 3);
                if (choise == 0 && Character.CheckAuraPoints(this, 50)) 
                    this.BOT.EnqueueAction("RB", 5);
                else if (choise == 1) 
                    this.BOT.EnqueueAction("C", 5);
                else 
                    this.BOT.EnqueueAction("A", 5);

            } else if (choise == 0 && Character.CheckAuraPoints(this, 100)) { // Super
                this.BOT.EnqueueAction("Down *", 5);
                this.BOT.EnqueueAction("Down *", 5);
                this.BOT.EnqueueAction("RB *", 5);
            } else {
                this.BOT.EnqueueAction("", 30);
            }
        }
    
        this.BOT.EnqueueAction("", AI.rand.Next(5 * this.BOT.difficulty, 25 * this.BOT.difficulty));
    }

    // Other
    public override void Reset(int start_point, int facing, String state = "Idle", bool total_reset = false) {
        base.Reset(start_point, facing, state, total_reset);
    }
    public void SpawnEffect(string state, float X, float Y, int facing, int X_offset = 0, int Y_offset = 0) {        
        RemyEffects re = new RemyEffects(this, state, X + X_offset * facing, Y + Y_offset, facing);
        re.Load();
        Program.stage?.newCharacters.Add(re);
    }
}