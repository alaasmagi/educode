import 'package:flutter/material.dart';

class SectionDivider extends StatelessWidget {
  final String? label;
  final double thickness;
  final double indent;
  final double endIndent;

  const SectionDivider({
    this.label,
    this.thickness = 1.0,
    this.indent = 0.0,
    this.endIndent = 0.0,
    Key? key,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    if (label != null && label!.isNotEmpty) {
      return Row(
        children: [
          Expanded(
            child: Divider(
              thickness: thickness,
              indent: indent,
            ),
          ),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            child: Text(
              label!,
              style: Theme.of(context).textTheme.bodySmall,
            ),
          ),
          Expanded(
            child: Divider(
              thickness: thickness,
              endIndent: endIndent,
            ),
          ),
        ],
      );
    }

    return Divider(
      thickness: thickness,
      indent: indent,
      endIndent: endIndent,
    );
  }
}

