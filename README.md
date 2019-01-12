# Xamarin.Android.SerialPort
Serial Port wrapper for Xamarin.Android. 

If you want to change some feature of the Serial Port, just clone this repo and change it. In the SerialPort project you have to go to the SerialPortWrapper folder and the SerialPort.cs.

After the update you have to Rebuild, and if you want to use the rerecently updatet SerialPort class, you have to add the dll (from bin/Release or Debug) to the target Project.

If you want to create your own SerialPort wrapper just go to https://github.com/chzhong/serial-android and open this project to Android Studio.
Create .aar file with grandle: go to grandle tab on the right of the Android Studio (:libserial/build/), select the package and search for clean (double click) and search for assembleRelease (double click).
The .aar file youe can find at the project folder and then the \libserial\build\outputs\aar.
Then create a Bining Xamarin Android Library, with the grandle (what you gave in the grandle) minimum adroid version, and add it to the Jar folder. (https://developer.xamarin.com/guides/android/advanced_topics/binding-a-java-library/binding-a-jar/ or https://developer.xamarin.com/guides/android/advanced_topics/binding-a-java-library/binding-an-aar/)
Rebuild. And that's it. Watch out for the namespaces. The namespace will be the java's namespace with upper case at the start.
