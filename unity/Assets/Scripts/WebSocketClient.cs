using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WebSocketClient : MonoBehaviour
{

    private static WebSocketClient instance;

    public string host = "localhost:3001";

    ClientWebSocket clientWebSocket;

    public TeleportToggle teleportToggle;

    public SliderController sliderController;

    public GameObject displayPointer;
    public GameObject bottomLeftDisplay;
    public GameObject topRightDisplay;
    public GameObject irPointer;
    public GameObject bottomLeftIR;
    public GameObject topRightIR;

    void Start()
    {
        instance = this;
        clientWebSocket = new ClientWebSocket();
        Connect();
    }

    async void Connect() {
        await clientWebSocket.ConnectAsync(new Uri("ws://" + host), CancellationToken.None);
        Debug.Log("Connected to websocket!");

        Receive(clientWebSocket);
    }

    private board_info display_board = new board_info {
        x = 0,
        y = 0,
        show = false
    };

    private board_info ir_board = new board_info {
        x = 0,
        y = 0,
        show = false
    };

    void HandleMessage(string msg) {
        //Debug.Log(msg);
        if (msg == "reset") {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        } else if (msg.StartsWith("frame")) {
            string[] split = msg.Split(',');
            string direction = split[1];

            if (direction == "next") {
                sliderController.NextFrame();
            } else if (direction == "last") {
                sliderController.LastFrame();
            }
        } if (msg == "teleport") {
            Debug.Log("teleporting player!");
            teleportToggle.toggle();
        } else if (msg.StartsWith("board")) {
            string[] lines = msg.Split('\n');

            string[] header = lines[0].Split(",");

            int board_indx = Array.IndexOf(header, "board");
            int x_indx = Array.IndexOf(header, "x");
            int y_indx = Array.IndexOf(header, "y");
            int show_indx = Array.IndexOf(header, "show");

            foreach (string line in lines) {
                string[] sline = line.Split(",");
                if (sline[board_indx] == "display") {
                    display_board.x = float.Parse(sline[x_indx]);
                    display_board.y = 1 - float.Parse(sline[y_indx]);

                    bool lastShow = display_board.show;

                    display_board.show = sline[show_indx] == "true";

                    if (lastShow != display_board.show) {
                        displayPointer.SetActive(display_board.show);
                    }

                    if (display_board.show) {
                        //x and y are percentages, find the position between bottom left and top right
                        float x = (display_board.x) * (topRightDisplay.transform.position.x - bottomLeftDisplay.transform.position.x);
                        float y = (display_board.y) * (topRightDisplay.transform.position.y - bottomLeftDisplay.transform.position.y);

                        x = x + bottomLeftDisplay.transform.position.x;
                        y = y + bottomLeftDisplay.transform.position.y;

                        displayPointer.transform.position = new Vector3(x, y, topRightDisplay.transform.position.z);
                    }
                } else if (sline[board_indx] == "ir-top-down") {
                    ir_board.x = float.Parse(sline[x_indx]);
                    ir_board.y = 1 - float.Parse(sline[y_indx]);

                    bool lastShow = ir_board.show;

                    ir_board.show = sline[show_indx] == "true";

                    if (lastShow != ir_board.show) {
                        irPointer.SetActive(ir_board.show);
                    }

                    if (ir_board.show) {
                        //x and y are percentages, find the position between bottom left and top right
                        float x = (ir_board.x) * (topRightIR.transform.position.x - bottomLeftIR.transform.position.x);
                        float y = (ir_board.y) * (topRightIR.transform.position.z - bottomLeftIR.transform.position.z);

                        x = x + bottomLeftIR.transform.position.x;
                        y = y + bottomLeftIR.transform.position.z;

                        irPointer.transform.position = new Vector3(x, topRightIR.transform.position.y, y);
                    }
                }
            }
        }
    }

    struct board_info {
        public float x;
        public float y;
        public bool show;
    }

    static async Task Receive(ClientWebSocket socket)
    {
        var buffer = new ArraySegment<byte>(new byte[2048]);
        do
        {
            WebSocketReceiveResult result;
            using (var ms = new MemoryStream())
            {
                do
                {
                    result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                ms.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(ms, Encoding.UTF8))
                    instance.HandleMessage(await reader.ReadToEndAsync());
            }
        } while (true);
    }
}
