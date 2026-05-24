public class Object {
    // Informações básicas
    public RigidBody body = new RigidBody();
    public int facing = 1;

    // Estado
    public bool active = true;
    public bool animate = true;
    public bool behave = true;
    public bool render = true;
    public bool remove = false;

    public Object() {}

    public virtual void Update() {
        if (!this.active) return;
    }
    public virtual void Behave() {
        if (!this.behave) return;
    }
    public virtual void Render(bool drawHitboxes = false) {
        if (!this.render) return;
    }
    public virtual void Animate() {
        if (!this.animate) return;
    }

    public virtual void Load(string Path) {}
    public virtual void Unload() {}
}
