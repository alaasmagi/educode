import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:mobile/router.dart';
import 'package:mobile/widgets/normal_button.dart';
import 'package:mobile/widgets/default_checkbox.dart';
import 'package:mobile/widgets/section_divider.dart';
import 'package:mobile/widgets/camera_preview.dart';
import 'package:mobile/widgets/form_text_field.dart';

import '../providers/login_provider.dart';

class TeacherHomeView extends ConsumerStatefulWidget {
  const TeacherHomeView({Key? key}) : super(key: key);

  @override
  ConsumerState<TeacherHomeView> createState() => _TeacherHomeViewState();
}

class _TeacherHomeViewState extends ConsumerState<TeacherHomeView> {
  final _formKey = GlobalKey<FormState>();
  String _studentId = '';
  bool _addTools = false;
  double _zoomLevel = 1.0;
  bool _isScanning = false;

  void _handleZoom(double zoom) {
    setState(() {
      _zoomLevel = zoom;
    });
  }

  void _handleSubmit() {
    final form = _formKey.currentState;
    if (form == null) return;
    if (!form.validate()) return;
    form.save();

    setState(() => _isScanning = true);

    Future.delayed(const Duration(seconds: 2), () {
      if (mounted) {
        setState(() => _isScanning = false);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Scanned ID: $_studentId')),
        );
      }
    });
  }

  void _openSettings() {
    showModalBottomSheet(
      context: context,
      builder: (context) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            ListTile(
              leading: const Icon(Icons.swap_horiz),
              title: const Text('Vaheta rolli'),
              onTap: () {
                Navigator.pop(context);
                router.go('/role-selection');
              },
            ),
            ListTile(
              leading: const Icon(Icons.logout),
              title: const Text('Logi välja'),
              onTap: () async {
                Navigator.pop(context);
                final loginStateNotifier = ref.read(loginStateProvider.notifier);
                await loginStateNotifier.clearUser();
                if (mounted) {
                  router.go('/login');
                }
              },
            ),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('EDUCODE'),
        actions: [
          IconButton(
            icon: const Icon(Icons.settings),
            onPressed: _openSettings,
          ),
        ],
      ),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Container(
                    width: 32,
                    height: 32,
                    decoration: BoxDecoration(
                      color: Theme.of(context).colorScheme.primary,
                      shape: BoxShape.circle,
                    ),
                    child: Center(
                      child: Text(
                        '1',
                        style: TextStyle(
                          color: Theme.of(context).colorScheme.onPrimary,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(width: 12),
                  const Text('Skänni tahvlil olev QR kood'),
                ],
              ),
              const SizedBox(height: 24),

              CameraPreview(
                height: 300,
                isScanning: _isScanning,
                zoomLevel: _zoomLevel,
                onZoomChanged: _handleZoom,
              ),
              const SizedBox(height: 24),
              const SectionDivider(label: 'Või sisesta ID käsitsi'),
              const SizedBox(height: 24),
              Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Ainetunni ID',
                      style: Theme.of(context).textTheme.titleSmall,
                    ),
                    const SizedBox(height: 8),
                    FormTextField(
                      fieldKey: const Key('student_id'),
                      label: 'nt. 123456-123456',
                      validator: (v) {
                        if (v == null || v.isEmpty) {
                          return 'Sisesta ID';
                        }
                        return null;
                      },
                      onSaved: (v) => _studentId = v ?? '',
                    ),
                    const SizedBox(height: 16),
                    DefaultCheckbox(
                      value: _addTools,
                      onChanged: (value) {
                        setState(() => _addTools = value ?? false);
                      },
                      label: 'Lisa töökoht',
                    ),
                    const SizedBox(height: 24),
                    NormalButton(
                      onPressed: _handleSubmit,
                      label: 'Jätka',
                      loading: _isScanning,
                      style: NormalButtonStyle.primary,
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}


