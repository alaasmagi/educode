import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:mobile/providers/login_provider.dart';
import 'package:mobile/widgets/normal_button.dart';
import 'package:mobile/widgets/default_checkbox.dart';
import 'package:mobile/widgets/section_divider.dart';
import 'package:mobile/widgets/camera_preview.dart';
import 'package:mobile/widgets/form_text_field.dart';
import 'package:mobile/widgets/language_switcher.dart';
import 'package:mobile/l10n/app_localizations.dart';
import 'package:mobile/widgets/theme_toggle.dart';

class StudentHomeView extends ConsumerStatefulWidget {
  const StudentHomeView({Key? key}) : super(key: key);

  @override
  ConsumerState<StudentHomeView> createState() => _StudentHomeViewState();
}

class _StudentHomeViewState extends ConsumerState<StudentHomeView> {
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
        final l10n = AppLocalizations.of(context)!;
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l10n.scannedID(_studentId))),
        );
      }
    });
  }

  void _openSettings() {
    final l10n = AppLocalizations.of(context)!;
    showModalBottomSheet(
      context: context,
      builder: (context) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const LanguageSwitcher(),
            const ThemeSwitcher(),
            ListTile(
              leading: const Icon(Icons.logout),
              title: Text(l10n.logout),
              onTap: () async {
                Navigator.pop(context);
                if (!mounted) return;

                final loginStateNotifier = ref.read(loginStateProvider.notifier);
                context.go('/login');
                await loginStateNotifier.clearUser();
              },
            ),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;

    return Scaffold(
      appBar: AppBar(
        title: Text(l10n.appTitle),
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
                  Text(l10n.scanQRCode),
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
              SectionDivider(label: l10n.orEnterIDManually),
              const SizedBox(height: 24),
              Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      l10n.lessonID,
                      style: Theme.of(context).textTheme.titleSmall,
                    ),
                    const SizedBox(height: 8),
                    FormTextField(
                      fieldKey: const Key('student_id'),
                      label: l10n.lessonIDPlaceholder,
                      validator: (v) {
                        if (v == null || v.isEmpty) {
                          return l10n.enterID;
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
                      label: l10n.addWorkspace,
                    ),
                    const SizedBox(height: 24),
                    NormalButton(
                      onPressed: _handleSubmit,
                      label: l10n.continueButton,
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


