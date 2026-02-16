class PasswordValidator {
  static String? validate(String? value) {
    if (value == null || value.isEmpty) {
      return 'Sisesta salasõna';
    }
    if (value.length < 8) {
      return 'Salasõna pikkus peab olema vähemalt 8 tähemärki';
    }
    return null;
  }
}

