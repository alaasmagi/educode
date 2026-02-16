// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for English (`en`).
class AppLocalizationsEn extends AppLocalizations {
  AppLocalizationsEn([String locale = 'en']) : super(locale);

  @override
  String get appTitle => 'EDUCODE';

  @override
  String get settings => 'Settings';

  @override
  String get logout => 'Log out';

  @override
  String get scanQRCode => 'Scan the QR code on the board';

  @override
  String get orEnterIDManually => 'Or enter ID manually';

  @override
  String get lessonID => 'Lesson ID';

  @override
  String get lessonIDPlaceholder => 'e.g. 123456-123456';

  @override
  String get enterID => 'Enter ID';

  @override
  String get addWorkspace => 'Add workspace';

  @override
  String get continueButton => 'Continue';

  @override
  String scannedID(String id) {
    return 'Scanned ID: $id';
  }

  @override
  String get language => 'Language';

  @override
  String get estonian => 'Estonian';

  @override
  String get english => 'English';

  @override
  String get theme => 'Theme';

  @override
  String get lightMode => 'Light';

  @override
  String get darkMode => 'Dark';

  @override
  String get systemDefault => 'System default';
}
