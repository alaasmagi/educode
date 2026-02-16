class EmailValidator {
  static String? validate(String? value) {
    if (value == null || value.isEmpty) {
      return 'Sisesta e-posti aadress';
    }
    if (!value.contains('@')) {
      return 'Sisesta kehtiv e-posti aadress';
    }
    return null;
  }
}

