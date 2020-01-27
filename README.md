# Phone-Face-Tablet-Controller for Social Robotics!

<img src="https://img.shields.io/badge/unity-2017.1.5f1-blue"/> <img src="https://img.shields.io/badge/platform-Android-green"/> <img src="https://img.shields.io/badge/supports-Misty II-orange"/> <img src="https://img.shields.io/badge/supports-Arduino-orange"/> <img src="https://img.shields.io/badge/license-GNU GPL v3-red"/> 

*Are you interested in social robots? Do you have two Android devices laying around? Do love cute little robot faces? <br/>
**Then this is the software for you!***

![Face with Romibo Eyes](/Images/FaceTrack.gif) <br/>
*Robot face with simple webcam face tracking enabled.*

## Summary <br/>
The *Phone-Face-Tablet-Controller for Social Robotics* is an open, expandable software framework compatible with a wide variety of social therapy robots for Windows and Android. Built in **Unity 2017.1.5** and a variety of commercial-off-the-shelf parts and plugins, this allows you to connect an Android tablet or phone displaying an animated robot's face paired with a Windows or Android laptop, tablet, or smart phone as the controller. The controller app, controlled by a human operator, sends commands to the face app that control Speech-To-Text, expression, face color, and more via either a Bluetooth or LAN WiFi connection. If connected to an Arduino USB interface, we can also control 2-wheeled base motion for moving forwards, backwards, and turning left and right. The ultimate use case of this software is to teach and inspire children with autism and other special needs.

![Tablet Controller Button Board](/Images/Buttons.PNG) <br/>
*Tablet Controller button board view. See list of Palettes on the left, button grid in middle, and individual Button Speech parameters on right.*

#### Phone Face Android Core Features: <br/>
1. Emotion Display ( Concerned | Surprise | Happy | Sad )
2. Face tracking with eyes looking at user.
3. Portrait or Landscape mode
4. Face Color Customization 
5. Speech to Text Voice
6. Lip Sync Vocals

#### Tablet Controller Android Core Features: <br/>
1. File Save, Load and Selection with speech phrase list as comma-delimited CSV
2. Speech to text entry, adding and deleting with quick phrase selection
3. Bluetooth discovery and connection of Client Face
4. Expression control / Face color control
5. Arduino robot hardware control

## USB Serial Arduino Tank Control <br/>
One of the coolest features of this social robot tablet controller phone face framework is that it allows the user to connect an Arduino-based board via USB directly to the phone face Android device *(given that the device has support for USB OTG devices.)* It is possible to send Arduino control signals from the tablet controller to the phone face either through LAN WiFi or Bluetooth, where the phone face is directly connected to the Arduino via USB OTG.

![Android-Android-Arduino Setup](/Images/Tank.jpg) <br/>
*A full example setup with tablet controller and phone face connected via serial USB to an Arduino-based tank platform.*

![Spinning the Tank Base](/Images/Spin.gif) <br/>
*Spinning the USB Arduino tank with Bluetooth Control from the tablet controller app!*

Download the Arduino Code for Robot Tank Base here: <br/>
https://github.com/johnchoi313/Phone-Face-Tablet-Controller/tree/master/ArduinoBluetoothTankControl

## Special Integrations (NEW!) <br/>

We've been working with the awesome team at **[Misty Robotics](https://www.mistyrobotics.com/)** to integrate the same tablet face controller app with their awesome line of social robots, in particular, the Misty I and Misty II.

![Misty II](/Images/mistyII.jpg)

The main HTTP REST API wrapper code we created for connecting Unity to the Misty I Robot (which can be re-used in other Unity C# projects) can be found here: <br/>
https://github.com/johnchoi313/Phone-Face-Tablet-Controller/tree/master/ArduinoBluetoothTankControl

## Compiled Download Links <br/>

Phone Face Compiled APK <br/>
https://github.com/johnchoi313/Phone-Face-Tablet-Controller/blob/master/APKs/FAM%20Tank%20Face.apk

Tablet Controller Compiled APK <br/>
https://github.com/johnchoi313/Phone-Face-Tablet-Controller/blob/master/APKs/FAM%20Tank%20Controller.apk

Misty I Controller Compiled APK <br/>
https://github.com/johnchoi313/Phone-Face-Tablet-Controller/blob/master/APKs/FAM%20Misty%20I%20Controller.apk

Misty II Controller Compiled APK <br/>
https://github.com/johnchoi313/Phone-Face-Tablet-Controller/blob/master/APKs/FAM%20Misty%20II%20Controller.apk

## Links to Third Party Plugins <br/>
There are quite a few third party plugins that were used to create this project, some free, and some paid. These plugins significantly expand the functionality of Unity (in particular, Unity for Android), that make this project possible. In order to fully compile this project correctly, you will need to download and load each of the following plugins in Unity:

WIZAPPLY CO., LTD. Serial Port Utility Pro ($78) <br/>
https://assetstore.unity.com/packages/tools/utilities/serial-port-utility-pro-125863

CROSSTALES LLC RT-Voice PRO ($65) <br/>
https://assetstore.unity.com/packages/tools/audio/rt-voice-pro-41068

UTMail - Email Composition and Sending Plugin ($25) <br/>
https://assetstore.unity.com/packages/tools/integration/utmail-email-composition-and-sending-plugin-90545

TECH TWEAKING Android & Microcontrollers / Bluetooth ($20) <br/>
https://assetstore.unity.com/packages/tools/input-management/android-microcontrollers-bluetooth-16467

SHATALMIC, LLC Bluetooth LE for iOS, tvOS and Android ($20) <br/>
https://assetstore.unity.com/packages/tools/network/bluetooth-le-for-ios-tvos-and-android-26661

PIOTR ZMUDZINSKI Mobile Speech Recognizer ($15) <br/>
https://assetstore.unity.com/packages/tools/audio/mobile-speech-recognizer-73036

SÜLEYMAN YASIR KULA Runtime File Browser (FREE) <br/>
https://assetstore.unity.com/packages/tools/gui/runtime-file-browser-113006

SÜLEYMAN YASIR KULA Simple Input System (FREE) <br/>
https://assetstore.unity.com/packages/tools/input-management/simple-input-system-113033

PAPER PLANE TOOLS OpenCV plus Unity (FREE) <br/>
https://assetstore.unity.com/packages/tools/integration/opencv-plus-unity-85928

CLAYTON INDUSTRIES Http Client (FREE) <br/>
https://assetstore.unity.com/packages/tools/network/http-client-79343

azixMcAze Unity-UIGradient (FREE) <br/>
https://github.com/azixMcAze/Unity-UIGradient

YOON CHANGSIK CSV2Table (FREE) <br/>
https://assetstore.unity.com/packages/tools/utilities/csv2table-36443

IMPHENZIA Gradient Sky (FREE) <br/>
https://assetstore.unity.com/packages/2d/textures-materials/sky/gradient-sky-109899

*(Note: We do not maintain or offer support for any of these third party plugins or libraries. The usage of each third party plugin and library is governed with respect to their own individual terms and licenses.)*

## Special Thanks <br/>
Special thanks to **[Fine Arts Miracles](https://fineartmiracles.com/)** and their wonderful work in discovering methodologies to implement social robotics for therapy and early childhood education for making this project possible! Additional special thanks to the incredible previous work of Origami Robotics and their open source Romibo robots in creating a sounding board for the application of social robots everywhere!

![The original Romibo robot](/Images/romibo.jpg) <br/>
*The original Romibo robot from Origami Robotics.*
