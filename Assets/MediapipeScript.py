import cv2
from cvzone.HandTrackingModule import HandDetector
import socket

# Parameters
width, height = 1280, 720
cap = cv2.VideoCapture(0)
cap.set(3, width)
cap.set(4, height)

# Detector
detector = HandDetector(maxHands=2, detectionCon=0.8)

# UDP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
serverAddressPort = ("127.0.0.1", 5052)

while True:
    success, img = cap.read()
    img = cv2.flip(img, 1)
    hands, img = detector.findHands(img)
    data = []

    if hands:
        for hand in hands:
            lmList = hand['lmList']
            handType = hand['type']  # "Left" atau "Right"
            data.append(handType)
            for lm in lmList:
                data.extend([lm[0], height - lm[1], lm[2]])
    else:
        data = ["None"]

    sock.sendto(str.encode(str(data)), serverAddressPort)

    img = cv2.resize(img, (0, 0), None, 0.5, 0.5)
    cv2.imshow("Image", img)
    cv2.waitKey(1)
