import 'package:flutter/material.dart';

class FormTextField extends StatelessWidget {
  final Key? fieldKey;
  final String label;
  final TextInputType keyboardType;
  final bool obscureText;
  final bool isEnabled;
  final String? placeHolder;
  final String? Function(String?)? validator;
  final void Function(String?)? onSaved;

  const FormTextField({
    this.fieldKey,
    required this.label,
    this.isEnabled = true,
    this.keyboardType = TextInputType.text,
    this.obscureText = false,
    this.validator,
    this.onSaved,
    this.placeHolder,
    Key? key,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return TextFormField(
      key: fieldKey,
      decoration: InputDecoration(
          labelText: label,
          hintText: placeHolder),
      enabled: isEnabled,
      keyboardType: keyboardType,
      obscureText: obscureText,
      validator: validator,
      onSaved: onSaved,
    );
  }
}

