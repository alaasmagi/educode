import 'package:flutter/material.dart';

enum NormalButtonStyle { primary, secondary }

class NormalButton extends StatelessWidget {
  final bool loading;
  final VoidCallback onPressed;
  final String label;
  final NormalButtonStyle style;
  final bool fullWidth;
  final bool isEnabled;

  const NormalButton({
    required this.onPressed,
    required this.label,
    this.isEnabled = true,
    this.loading = false,
    this.style = NormalButtonStyle.primary,
    this.fullWidth = true,
    Key? key,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    if (loading) {
      return const Center(child: CircularProgressIndicator());
    }

    final button = style == NormalButtonStyle.primary
        ? FilledButton(
            onPressed: isEnabled ? onPressed : null,
            child: Text(label),
          )
        : OutlinedButton(
            onPressed: isEnabled ? onPressed : null,
            child: Text(label),
          );

    return fullWidth
        ? SizedBox(width: double.infinity, child: button)
        : button;
  }
}

