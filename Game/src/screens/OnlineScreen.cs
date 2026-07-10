public class WGOnlineSender : Widget {
    public override void Render() {
        // Desenha as sprites que recebeu

        // Envia o input
        OnlineInput.SendLocalInput(Input.currentInput[1]);
    }
}