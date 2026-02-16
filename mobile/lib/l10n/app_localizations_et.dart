// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for Estonian (`et`).
class AppLocalizationsEt extends AppLocalizations {
  AppLocalizationsEt([String locale = 'et']) : super(locale);

  @override
  String get appTitle => 'EDUCODE';

  @override
  String get settings => 'Seaded';

  @override
  String get logout => 'Logi välja';

  @override
  String get scanQRCode => 'Skänni tahvlil olev QR kood';

  @override
  String get orEnterIDManually => 'Või sisesta ID käsitsi';

  @override
  String get lessonID => 'Ainetunni ID';

  @override
  String get lessonIDPlaceholder => 'nt. 123456-123456';

  @override
  String get enterID => 'Sisesta ID';

  @override
  String get addWorkspace => 'Lisa töökoht';

  @override
  String get continueButton => 'Jätka';

  @override
  String scannedID(String id) {
    return 'Skännitud ID: $id';
  }

  @override
  String get language => 'Keel';

  @override
  String get estonian => 'Eesti';

  @override
  String get english => 'English';

  @override
  String get theme => 'Teema';

  @override
  String get lightMode => 'Hele';

  @override
  String get darkMode => 'Tume';

  @override
  String get systemDefault => 'Süsteemi vaikimisi';
}
