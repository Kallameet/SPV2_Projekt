package reinhard.sensorvaluesapp;

import android.util.Log;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.net.InetAddress;
import java.net.Socket;

public class TcpClient {

    private String mServerIp;
    private int mServerPort;

    private boolean mRun = false;
    private PrintWriter mBufferOut;

    public TcpClient(String serverIp, int serverPort) {
        mServerIp = serverIp;
        mServerPort = serverPort;
    }

    public void sendMessage(SensorValues sensorValues) {
        if (mBufferOut != null && !mBufferOut.checkError() && sensorValues != null) {
            try {
                XmlCreator creator = new XmlCreator();
                mBufferOut.write(creator.CreateXmlFromSensorValues(sensorValues));
            }
            catch (Exception e) {
                Log.e("Serialization", "S: Error", e);
            }
        }
    }

    public void stop() {
        Log.i("Debug", "stop");

        mRun = false;

        if (mBufferOut != null) {
            mBufferOut.flush();
            mBufferOut.close();
        }

        mBufferOut = null;
    }

    public void run() {

        mRun = true;

        try {
            InetAddress serverAddr = InetAddress.getByName(mServerIp);
            Log.i("TCP Client", "C: Connecting...");
            Log.i("Server Ip", serverAddr.getHostAddress());
            Log.i("Server Port", Integer.toString(mServerPort));

            Socket socket = new Socket(serverAddr, mServerPort);

            try {
                Log.i("Debug", "inside try catch");
                mBufferOut = new PrintWriter(new BufferedWriter(new OutputStreamWriter(socket.getOutputStream())), true);
                while (mRun) {
                }
            } catch (Exception e) {
                Log.e("TCP", "S: Error", e);
            } finally {
                socket.close();
            }

        } catch (Exception e) {

            Log.e("TCP", "C: Error", e);
        }
    }

    public interface OnMessageReceived {
        public void messageReceived(String message);
    }
}

