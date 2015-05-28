package reinhard.sensorvaluesapp;

import android.util.Log;

import org.simpleframework.xml.Attribute;
import org.simpleframework.xml.Element;
import org.simpleframework.xml.Root;
import org.simpleframework.xml.Serializer;
import org.simpleframework.xml.core.Persister;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.net.InetAddress;
import java.net.Socket;

public class TcpClient {

    private String mServerIp;
    private int mServerPort;

    // message to send to the server
    private String mServerMessage;
    // sends message received notifications
    //private OnMessageReceived mMessageListener = null;
    // while this is true, the server will continue running
    private boolean mRun = false;
    // used to send messages
    private PrintWriter mBufferOut;
    // used to read messages from the server
    private BufferedReader mBufferIn;

    /**
     * Constructor of the class. OnMessagedReceived listens for the messages received from server
     */
    public TcpClient(String serverIp, int serverPort) {
        //mMessageListener = listener;
        mServerIp = serverIp;
        mServerPort = serverPort;
    }

     // Sends the message entered by client to the server
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

    /**
     * Close the connection and release the members
     */
    public void stop() {
        Log.i("Debug", "stop");

        mRun = false;

        if (mBufferOut != null) {
            mBufferOut.flush();
            mBufferOut.close();
        }

        //mMessageListener = null;
        mBufferIn = null;
        mBufferOut = null;
        mServerMessage = null;
    }

    public void run() {

        mRun = true;

        try {
            //here you must put your computer's IP address.
            InetAddress serverAddr = InetAddress.getByName(mServerIp);

            Log.i("TCP Client", "C: Connecting...");

            Log.i("Server Ip", serverAddr.getHostAddress());
            Log.i("Server Port", Integer.toString(mServerPort));

            //create a socket to make the connection with the server
            Socket socket = new Socket(serverAddr, mServerPort);

            try {
                Log.i("Debug", "inside try catch");
                //sends the message to the server
                mBufferOut = new PrintWriter(new BufferedWriter(new OutputStreamWriter(socket.getOutputStream())), true);

                //receives the message which the server sends back
                mBufferIn = new BufferedReader(new InputStreamReader(socket.getInputStream()));
                // send login name
                //sendMessage(Constants.LOGIN_NAME + PreferencesManager.getInstance().getUserName());
                //sendMessage("Hi");
                //in this while the client listens for the messages sent by the server
                while (mRun) {
                    mServerMessage = mBufferIn.readLine();
                    //if (mServerMessage != null && mMessageListener != null) {
                    //    //call the method messageReceived from MyActivity class
                    //    mMessageListener.messageReceived(mServerMessage);
                    //}

                }
                Log.e("RESPONSE FROM SERVER", "S: Received Message: '" + mServerMessage + "'");

            } catch (Exception e) {

                Log.e("TCP", "S: Error", e);

            } finally {
                //the socket must be closed. It is not possible to reconnect to this socket
                // after it is closed, which means a new socket instance has to be created.
                socket.close();
            }

        } catch (Exception e) {

            Log.e("TCP", "C: Error", e);

        }

    }

    //Declare the interface. The method messageReceived(String message) will must be implemented in the MyActivity
    //class at on asynckTask doInBackground
    public interface OnMessageReceived {
        public void messageReceived(String message);
    }
}

