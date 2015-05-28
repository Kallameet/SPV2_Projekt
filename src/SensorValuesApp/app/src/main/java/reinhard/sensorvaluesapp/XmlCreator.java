package reinhard.sensorvaluesapp;

/**
 * Created by Reinhard on 28.05.2015.
 */
public class XmlCreator {
    private String GetHeader()
    {
        return "<?xml version=\"1.0\" encoding=\"ibm850\"?>\r\n";
    }

    public String CreateTags(String tag, String value)
    {
        return String.format("<%s>%s</%s>\r\n", tag, value, tag);
    }

    public String CreateXmlFromSensorValues(SensorValues values)
    {
        String header = GetHeader();
        String xmlValues = String.format("  %s  %s  %s", CreateTags("AccelerometerX", Float.toString(values.getAccelerometerX())),
                CreateTags("AccelerometerY", Float.toString(values.getAccelerometerY())),
                CreateTags("AccelerometerZ", Float.toString(values.getAccelerometerZ())));
        return String.format("%s<SensorValues xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n%s</SensorValues>\r\n", header, xmlValues);
    }
}
