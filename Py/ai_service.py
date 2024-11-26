from fastapi import FastAPI, File, UploadFile
from fastapi.responses import JSONResponse
from ultralytics import YOLO
import cv2
import numpy as np
import os

# Tạo FastAPI app
app = FastAPI()

# Load YOLO model (sử dụng model đã huấn luyện của bạn)
model = YOLO("yolov8n.pt")  # Thay yolov8n.pt bằng model YOLO đã huấn luyện

@app.post("/detect/")
async def detect(file: UploadFile = File(...)):
    try:
        # Lưu ảnh tạm thời
        temp_file = f"temp_{file.filename}"
        with open(temp_file, "wb") as f:
            f.write(await file.read())

        # Nhận diện ảnh bằng YOLO
        results = model(temp_file)
        predictions = []
        for result in results:
            for box in result.boxes:
                predictions.append({
                    "label": model.names[int(box.cls[0])],
                    "confidence": float(box.conf[0]),
                    "bbox": [int(coord) for coord in box.xyxy[0].tolist()]
                })

        # Xóa file tạm
        os.remove(temp_file)

        return JSONResponse(content={"predictions": predictions}, status_code=200)
    except Exception as e:
        return JSONResponse(content={"error": str(e)}, status_code=500)
