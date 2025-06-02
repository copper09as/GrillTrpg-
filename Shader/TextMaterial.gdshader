shader_type canvas_item;

// 雷电文字参数
uniform float intensity : hint_range(0, 5) = 1.5;       // 特效强度
uniform float speed : hint_range(0, 5) = 2.0;          // 动画速度
uniform vec4 primary_color = vec4(1.0, 0.8, 0.2, 1.0); // 主色调 (金色)
uniform vec4 secondary_color = vec4(0.2, 0.8, 1.0, 1.0); // 次色调 (亮蓝色)
uniform sampler2D noise_tex : source_color;             // 噪声纹理
uniform float glow_size : hint_range(0, 3) = 1.0;       // 辉光大小
uniform float branch_density : hint_range(0, 10) = 3.0; // 分支密度
uniform float color_shift_speed : hint_range(0, 2) = 0.8; // 颜色变化速度
uniform bool use_alpha_test = true;       // 是否使用alpha测试
uniform float alpha_threshold : hint_range(0, 0.1) = 0.001; // alpha测试阈值

// 随机函数
float rand(vec2 co) {
    return fract(sin(dot(co.xy, vec2(12.9898,78.233))) * 43758.5453);
}

// 旋转函数
vec2 rotateUV(vec2 uv, float rotation) {
    float mid = 0.5;
    return vec2(
        cos(rotation) * (uv.x - mid) + sin(rotation) * (uv.y - mid) + mid,
        cos(rotation) * (uv.y - mid) - sin(rotation) * (uv.x - mid) + mid
    );
}

void fragment() {
    vec4 base_color = texture(TEXTURE, UV);
    
    // 仅在有文字的区域应用效果
    bool apply_effect = !use_alpha_test || base_color.a > alpha_threshold;
    
    if (apply_effect) {
        // 创建时间变量
        float t = TIME * speed;
        
        // 颜色变化效果
        float color_shift = sin(t * color_shift_speed * 2.0) * 0.5 + 0.5;
        vec4 lightning_color = mix(primary_color, secondary_color, color_shift);
        
        // 主噪声层 - 基础闪电形状
        vec2 uv1 = UV * vec2(1.5, 1.2) + vec2(t * 0.15, t * 0.2);
        float noise1 = texture(noise_tex, uv1).r;
        noise1 = pow(noise1, 2.0);
        
        // 次噪声层 - 增加细节
        vec2 uv2 = UV * vec2(2.0, 1.8) - vec2(t * 0.2, t * 0.18);
        float noise2 = texture(noise_tex, uv2).g;
        noise2 = pow(noise2, 2.0);
        
        // 计算边缘距离 (0=边缘, 1=中心)
        float edge_dist = min(
            min(UV.x, 1.0 - UV.x),
            min(UV.y, 1.0 - UV.y)
        );
        
        // 创建主闪电效果
        float main_lightning = smoothstep(0.3, 0.7, noise1) *
                              smoothstep(0.2, 0.6, noise2) *
                              (1.0 - smoothstep(0.0, 0.15, edge_dist));
        main_lightning = pow(main_lightning, 0.7);
        
        // 添加闪电分支
        float branches = 0.0;
        for (float i = 0.0; i < 3.0; i++) {
            float branch_offset = i * 0.4;
            vec2 branch_uv = rotateUV(UV, 0.3 * sin(t * 0.8 + i));
            branch_uv += vec2(sin(t * 1.0 + i), cos(t * 0.9 + i)) * 0.15;
            
            float branch_noise1 = texture(noise_tex, branch_uv * (1.8 + i * 0.3) + vec2(t * 0.15, t * 0.12 + branch_offset)).r;
            float branch_noise2 = texture(noise_tex, branch_uv * (2.5 - i * 0.2) - vec2(t * 0.18, t * 0.15 + branch_offset)).g;
            
            float branch_value = smoothstep(0.2, 0.7, branch_noise1 * branch_noise2) * 
                               (1.0 - smoothstep(0.0, 0.2, edge_dist)) * 
                               (0.6 + 0.4 * sin(t * 3.0 + i));
            
            branches += branch_value * (1.0 + i * 0.3);
        }
        branches *= branch_density * 0.3;
        
        // 组合闪电
        float lightning = (main_lightning * 1.5 + branches) * intensity;
        
        // 创建闪电核心
        vec3 core_lightning = vec3(pow(lightning, 0.6)) * lightning_color.rgb * 2.0;
        
        // 创建辉光效果
        float glow = smoothstep(0.1, 0.7, lightning) * glow_size;
        vec3 glow_color = mix(lightning_color.rgb * 0.7, vec3(1.0), 0.8);
        vec3 glow_effect = glow_color * pow(glow, 1.5) * 1.2;
        
        // 混合所有效果
        vec3 final_color = base_color.rgb;
        
        // 使用更直观的混合方式
        final_color = mix(final_color, lightning_color.rgb, lightning * 0.2);
        final_color += core_lightning * base_color.a;
        final_color += glow_effect * base_color.a;
        
        // 限制颜色值范围
        final_color = clamp(final_color, 0.0, 1.0);
        
        // 保持原始透明度
        COLOR = vec4(final_color, base_color.a);
    } else {
        COLOR = base_color;
    }
}