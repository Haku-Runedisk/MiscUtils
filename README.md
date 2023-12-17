# MiscUtils
## Usage instructions
- Custom info:
  Add `{Q.CustomInfoStr()}` to TAS custom info and configure content using mod options and hotkeys. Some are:
    - Show rounding error:
        See player offset off of hardcoded x & y position grids (1/360 px, 1/120 px), converted to a different unit. The unit, u, is chosen such that moving at -105 intended speed produces -105u DeltaTime-based error. Acceleration DeltaTime error and float error make it different in practice. Practical examples of conversion, intended speed -> rounding error:
        - jump, y: -105.00 -> -106.26u
        - fall, y: 160.00 -> 156.34u
        - peak after hyper, y: 0.00 -> 54.05u
        - peak after super, y: 0.00 -> 109.01u
        - jump of 7f hyper bhop, x: 328.00 -> 279.45u
## Known issues
- This list is not exhaustive, but at least includes:
	- Settings don't save reliably on closing.
	- Hotkeys don't work during TAS.
