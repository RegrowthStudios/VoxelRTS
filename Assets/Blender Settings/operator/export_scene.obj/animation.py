import bpy
op = bpy.context.active_operator

op.filepath = 'C:\\untitled.obj'
op.use_selection = True
op.use_animation = True
op.use_apply_modifiers = True
op.use_edges = False
op.use_normals = False
op.use_uvs = False
op.use_materials = False
op.use_triangles = True
op.use_nurbs = False
op.use_vertex_groups = False
op.use_blen_objects = False
op.group_by_object = False
op.group_by_material = False
op.keep_vertex_order = False
op.global_scale = 1.0
op.axis_forward = 'X'
op.axis_up = 'Y'
op.path_mode = 'AUTO'
