import 'package:flutter/material.dart';
import 'package:camera/camera.dart' as cam;

class CameraPreview extends StatefulWidget {
  final double height;
  final bool isScanning;
  final double zoomLevel;
  final ValueChanged<double> onZoomChanged;

  const CameraPreview({
    this.height = 300,
    this.isScanning = false,
    this.zoomLevel = 1.0,
    required this.onZoomChanged,
    super.key,
  });

  @override
  State<CameraPreview> createState() => _CameraPreviewState();
}

class _CameraPreviewState extends State<CameraPreview> {
  cam.CameraController? _controller;
  List<cam.CameraDescription>? _cameras;
  bool _isInitialized = false;
  String? _errorMessage;

  @override
  void initState() {
    super.initState();
    _initializeCamera();
  }

  Future<void> _initializeCamera() async {
    try {
      _cameras = await cam.availableCameras();
      if (_cameras == null || _cameras!.isEmpty) {
        setState(() {
          _errorMessage = 'No cameras available';
        });
        return;
      }

      final camera = _cameras!.firstWhere(
        (camera) => camera.lensDirection == cam.CameraLensDirection.back,
        orElse: () => _cameras!.first,
      );

      _controller = cam.CameraController(
        camera,
        cam.ResolutionPreset.high,
        enableAudio: false,
      );

      await _controller!.initialize();

      if (mounted) {
        setState(() {
          _isInitialized = true;
        });
      }
    } catch (e) {
      setState(() {
        _errorMessage = 'Error initializing camera: $e';
      });
    }
  }

  @override
  void dispose() {
    _controller?.dispose();
    super.dispose();
  }

  @override
  void didUpdateWidget(CameraPreview oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (widget.zoomLevel != oldWidget.zoomLevel && _controller != null) {
      _controller!.setZoomLevel(widget.zoomLevel);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        Container(
          width: double.infinity,
          height: widget.height,
          decoration: BoxDecoration(
            color: Colors.black,
            borderRadius: BorderRadius.circular(16),
            border: Border.all(
              color: Theme.of(context).dividerColor,
              width: 2,
            ),
          ),
          child: ClipRRect(
            borderRadius: BorderRadius.circular(14),
            child: _buildCameraContent(),
          ),
        ),
        Positioned(
          right: 16,
          top: 16,
          child: Column(
            children: [
              Container(
                width: 40,
                height: 40,
                decoration: BoxDecoration(
                  color: Colors.black54,
                  shape: BoxShape.circle,
                ),
                child: const Icon(
                  Icons.search,
                  color: Colors.white,
                  size: 24,
                ),
              ),
              const SizedBox(height: 8),
              _ZoomButton(
                label: '1x',
                isSelected: widget.zoomLevel == 1.0,
                onPressed: () => widget.onZoomChanged(1.0),
              ),
              const SizedBox(height: 8),
              _ZoomButton(
                label: '2x',
                isSelected: widget.zoomLevel == 2.0,
                onPressed: () => widget.onZoomChanged(2.0),
              ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildCameraContent() {
    if (widget.isScanning) {
      return const Center(
        child: CircularProgressIndicator(color: Colors.white),
      );
    }

    if (_errorMessage != null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(16.0),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Icon(
                Icons.error_outline,
                size: 48,
                color: Colors.red,
              ),
              const SizedBox(height: 16),
              Text(
                _errorMessage!,
                style: const TextStyle(color: Colors.white),
                textAlign: TextAlign.center,
              ),
            ],
          ),
        ),
      );
    }

    if (!_isInitialized || _controller == null) {
      return const Center(
        child: CircularProgressIndicator(color: Colors.white),
      );
    }

    // Use FittedBox to scale the camera preview correctly
    return FittedBox(
      fit: BoxFit.cover,
      child: SizedBox(
        width: _controller!.value.previewSize!.height,
        height: _controller!.value.previewSize!.width,
        child: cam.CameraPreview(_controller!),
      ),
    );
  }
}

class _ZoomButton extends StatelessWidget {
  final String label;
  final bool isSelected;
  final VoidCallback onPressed;

  const _ZoomButton({
    required this.label,
    required this.isSelected,
    required this.onPressed,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 48,
      height: 48,
      decoration: BoxDecoration(
        color: isSelected ? Colors.white : Colors.black54,
        borderRadius: BorderRadius.circular(8),
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: onPressed,
          borderRadius: BorderRadius.circular(8),
          child: Center(
            child: Text(
              label,
              style: TextStyle(
                color: isSelected ? Colors.black : Colors.white,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
        ),
      ),
    );
  }
}

