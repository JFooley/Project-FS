public class WGOnlineSender : Widget {
    public override void Render() {
        // Desenha as sprites que recebeu
        OnlineInput.RenderFrame();

        // Envia o input
        OnlineInput.SendInput(Input.currentInput[1]);
    }
}