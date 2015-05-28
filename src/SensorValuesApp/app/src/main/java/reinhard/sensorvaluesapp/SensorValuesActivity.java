package reinhard.sensorvaluesapp;

import android.app.Activity;
import android.content.Context;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.os.AsyncTask;
import android.os.Bundle;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;


public class SensorValuesActivity extends Activity implements SensorEventListener {

    private SensorManager senSensorManager;
    private Sensor senAccelerometer;

    private long lastUpdate = 0;
    private float last_x, last_y, last_z;
    private static final int SHAKE_THRESHOLD = 600;

    private Button connectButton;
    private Button disconnectButton;

    TcpClient tcpClient;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_sensor_values);

        senSensorManager = (SensorManager) getSystemService(Context.SENSOR_SERVICE);
        senAccelerometer = senSensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER);
        senSensorManager.registerListener(this, senAccelerometer , SensorManager.SENSOR_DELAY_NORMAL);

        connectButton = (Button) findViewById(R.id.btnConnect);
        //connectButton.setEnabled(true);
        disconnectButton = (Button) findViewById(R.id.btnDisconnect);
        //disconnectButton.setEnabled(false);
    }

    protected void onPause() {
        super.onPause();
        senSensorManager.unregisterListener(this);
    }

    protected void onResume() {
        super.onResume();
        senSensorManager.registerListener(this, senAccelerometer, SensorManager.SENSOR_DELAY_NORMAL);
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.sensor_values, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();
        if (id == R.id.action_settings) {
            return true;
        }
        return super.onOptionsItemSelected(item);
    }

    @Override
    public void onSensorChanged(SensorEvent event) {
        Sensor mySensor = event.sensor;

        if (mySensor.getType() == Sensor.TYPE_ACCELEROMETER) {
            SensorValues sensorValues = new SensorValues();
            sensorValues.setAccelerometerX(event.values[0]);
            sensorValues.setAccelerometerY(event.values[1]);
            sensorValues.setAccelerometerZ(event.values[2]);

            long curTime = System.currentTimeMillis();

            if ((curTime - lastUpdate) > 100) {
                long diffTime = (curTime - lastUpdate);
                lastUpdate = curTime;

                TextView textView = (TextView) findViewById(R.id.lblAccelerometerValues);
                textView.setText(String.format("X: %s, Y: %s, Z: %s", sensorValues.getAccelerometerX(), sensorValues.getAccelerometerY(), sensorValues.getAccelerometerZ()));

                // check if client is connected
                if (tcpClient != null) {
                    tcpClient.sendMessage(sensorValues);
                }
            }
        }
    }

    @Override
    public void onAccuracyChanged(Sensor sensor, int accuracy) {

    }

    public void OnClickConnect(View view) {
        new ConnectTask().execute("");

        // TODO: check if connected

        disconnectButton.setEnabled(true);
        connectButton.setEnabled(false);
    }

    public void OnClickDisconnect(View view) {
        tcpClient.stop();
        connectButton.setEnabled(true);
        disconnectButton.setEnabled(false);
    }

    public class ConnectTask extends AsyncTask<String, String, TcpClient> {

        @Override
        protected TcpClient doInBackground(String... message) {
            EditText serverIp = (EditText) findViewById(R.id.tbServerIp);
            EditText serverPort = (EditText) findViewById(R.id.tbServerPort);
            tcpClient = new TcpClient(serverIp.getText().toString(), Integer.parseInt(serverPort.getText().toString()));
            tcpClient.run();

            return null;
        }
    }
}
