import 'package:flutter/material.dart';

class EmailWithDomainField extends StatefulWidget {
  final String domain;
  final String? initialLocalPart;
  final String? Function(String?)? validator;
  final void Function(String?)? onSaved;
  final bool isEnabled;

  const EmailWithDomainField({
    required this.domain,
    this.initialLocalPart,
    this.validator,
    this.onSaved,
    this.isEnabled = true,
    Key? key,
  }) : super(key: key);

  @override
  State<EmailWithDomainField> createState() => _EmailWithDomainFieldState();
}

class _EmailWithDomainFieldState extends State<EmailWithDomainField> {
  late TextEditingController _controller;

  @override
  void initState() {
    super.initState();
    _controller = TextEditingController(text: widget.initialLocalPart);
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return TextFormField(
      controller: _controller,
      enabled: widget.isEnabled,
      decoration: InputDecoration(
        labelText: 'E-posti aadress',
        hintText: 'Sisesta oma e-posti aadressi esimene osa',
        suffixText: '@${widget.domain}',
        suffixStyle: TextStyle(
          color: Theme.of(context).textTheme.bodyLarge?.color,
          fontWeight: FontWeight.w500,
        ),
      ),
      keyboardType: TextInputType.emailAddress,
      validator: widget.validator,
      onSaved: widget.onSaved,
    );
  }
}

